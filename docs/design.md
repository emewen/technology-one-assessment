# Design

## The problem

Convert an amount of money into words. `123.45` becomes
`ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS`.

The conversion happens on the server. The browser posts whatever the user typed to
`POST /api/conversions` and displays the answer it gets back.

## Projects

| Project | Role |
| --- | --- |
| `NumberToWords.Domain` | Parsing and conversion. The whole algorithm. |
| `NumberToWords.Api` | The endpoint and the page. |
| `NumberToWords.Domain.Tests` | Unit tests. |
| `NumberToWords.Api.Tests` | Integration tests. |

The domain project has no dependencies. No ASP.NET Core types reach it and it pulls in no NuGet
package. The algorithm can therefore be tested without starting a web server, and 31 of the
42 tests finish in well under a second because of it. The same library would also drop
into a console application or a nightly batch job without a line changing.

Everything HTTP lives in the API project: routing, model binding, error responses, logging,
rate limiting, static files.

## How the conversion works

### Groups of three

English names large numbers in groups of three digits. Each group reads the same way wherever
it sits, and a scale word says which group it is. `1,234,567` is one *million*, two hundred and
thirty-four *thousand*, five hundred and sixty-seven.

The converter mirrors that structure. Split the digits into threes from the right. Name each
group with a single routine covering 1 to 999. Append the scale word. Groups holding zero are
skipped, which stops `1,000,000` reading as `ONE MILLION ZERO THOUSAND ZERO`.

Solving 1 to 999 once puts every awkward part of English naming in one method. Numbers under
twenty come from a lookup table, because English names them individually and no rule derives
`TWELVE` from `TWO`. Tens come from a second table. A hyphen joins the halves: `FORTY-FIVE`.
Above a thousand none of that logic is needed again.

Supporting a larger scale means adding one string to a table. Ten entries currently reach the
octillions.

### The word "AND"

Both placements follow Australian and British usage.

Inside a group, "AND" follows the hundreds figure. `123` is `ONE HUNDRED AND TWENTY-THREE`. The
example in the brief uses this form, which rules out American usage where the word vanishes.

Across groups, "AND" precedes a trailing remainder under one hundred. `1001` is
`ONE THOUSAND AND ONE`. Once that remainder reaches a hundred the word disappears again, so
`1100` is `ONE THOUSAND ONE HUNDRED`.

### Dollars and cents

`MoneyToWordsConverter` decides which parts appear, whether each unit name is singular or
plural, and where "AND" separates them. Everything else it delegates. The numbers come from the
injected converter. The words DOLLAR and CENT arrive in a `CurrencyNames` record.

An empty part disappears from the answer. `0.45` is `FORTY-FIVE CENTS`, not
`ZERO DOLLARS AND FORTY-FIVE CENTS`. `5.00` is `FIVE DOLLARS`. Dropping both parts of zero
would leave an empty string, so zero keeps the whole part and reads `ZERO DOLLARS`.

Because the unit names are data, a second currency needs no new conversion code. A test
converts an amount to pounds and pence, so that claim is checked and not merely asserted.

## Alternatives I rejected

### Storing the amount

`Money` keeps the whole part as a string of digits and the cents as an integer from 0 to 99.
The obvious numeric types all failed for different reasons.

`double` is out before anything else gets considered. Binary floating point cannot represent
most decimal fractions. Store `0.45` and you get a value near 0.45, and the cents you read back
may well be 44.

`decimal` fixes the accuracy problem and then fails on range. It stores 29 significant digits
and quietly rounds anything longer. The largest supported amount would then be a property of a
framework type instead of a decision I get to make.

`long` tops out near 9.2 quintillion. The scale names reach nine orders of magnitude past that,
so the ceiling would be arbitrary.

`BigInteger` has no ceiling. It does give the converter a number that has to be divided
straight back into three-digit groups. The algorithm reads digits, so turning text into a
number and the number back into digits is wasted work.

Digit strings have none of those problems, and the only limit left is one I chose: 30 digits,
because the scale table holds ten entries. `Money.MaxWholeUnitDigits` and the converter's
`MaxDigits` have to agree: raise the parser limit without adding a scale name and input would
reach a converter that cannot name it.

### Reading the input

`decimal.TryParse` returns one boolean for every kind of failure. The page could then say only
that something was wrong, never what. It also accepts exponent notation, so `1e5` would slip
through as an amount. It rounds silently past 29 digits. And its behaviour follows the current
culture, which makes `1,234` mean different things in different regions.

A regular expression accepts the same grammar in one line, and a failed match is still one
boolean. Naming the offending character, or distinguishing three decimal places from a stray
letter, would take several patterns tried in sequence. That is harder to follow than the loop
it would replace.

The hand-written parser runs to about sixty lines and names a reason for every rejection. The
page shows that reason to whoever typed the amount, and a client can branch on the stable code
beside it.

The parser strips whitespace, commas and a leading dollar sign before it reads anything.
Someone pasting
`1,234.56` out of a spreadsheet means what someone typing `1234.56` means. Checking that every
comma sits three digits from the last would reject that for no benefit.

### Rounding

`1.234` comes back as a refusal naming the problem. Rounding it to `1.23` would change a
customer's figure without telling them.

### Exceptions for bad input

`MoneyParser.Parse` returns `Result<Money>`, holding either an amount or a `ValidationError`.
Mistyped input is ordinary traffic on a public form. Handling it takes a branch instead of a
throw and a catch. Callers have to acknowledge the failure path before they can reach the
value.

Exceptions still guard programmer error. `Money.Create` throws if handed a whole part
containing a letter. No user input can arrive in that state. A caller that manages it has a
bug.

### A number in the request body

`ConvertAmountRequest` binds `Value` as a string. Binding it to a `decimal` would hand the
framework's parser the exact decisions this service exists to make: how many decimal places to
accept, what to do with input it cannot read. The service sees what the user typed, character
for character.

### Third-party packages

Neither `src` project references a NuGet package. The test projects use xUnit and the ASP.NET
Core in-memory host, and neither touches the conversion.

`StringBuilder` comes from `System.Text`. The invariant culture used when reading a three-digit
group comes from `System.Globalization`. Nothing else outside the base class library appears
anywhere.

## The API

One endpoint does the work.

```
POST /api/conversions
{ "value": "123.45" }

200 OK
{
  "input": "123.45",
  "amount": "123.45",
  "words": "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS"
}
```

A refusal returns 400 and an RFC 9457 problem document. The reason code goes in the `code`
extension, so a client matches on `input.too_many_decimal_places` and never on the wording of
the message.

```
400 Bad Request
{
  "title": "The amount could not be read.",
  "status": 400,
  "detail": "Enter at most two decimal places. Amounts are not rounded for you.",
  "code": "input.too_many_decimal_places"
}
```

The response sends back both the raw input and the amount the parser read. Type `1,234.5` and
you see `1234.50`, which confirms the service understood you before you trust the words.

`GET /health` returns `Healthy` for the App Service probe and the deployment check.

### Around the endpoint

A global exception handler maps an unreadable body to 400 and everything else to 500. Neither
response includes an exception message or a stack trace, and an integration test checks that.
A stack trace on the wire helps an attacker and tells the user nothing.

Every response gets security headers. The content security policy permits this origin only.
That rules out inline script and inline style, which is why the stylesheet and the script are
separate files.

Rate limiting caps each client address at 120 requests a minute. The endpoint is cheap, so this
is about protecting the free tier's daily quota from one noisy caller.

Logging goes through source-generated `LoggerMessage` delegates. A refusal logs its reason code
at information level. The input itself is never logged.

Forwarded headers matter because App Service terminates TLS at its front end. Without them
the application sees neither the real scheme nor the client address.

## The page

Hand-written HTML, CSS and JavaScript, served as static files. No framework, no CDN. That keeps
the page inside its own content security policy and removes every third party from the supply
chain.

The field is `type="text"` with `inputmode="decimal"`. A `type="number"` input silently discards
values the browser dislikes, strips pasted commas, and disagrees with other browsers about what
it will hold. Reading a plain string leaves the server as the only judge of the input.

Accessibility: the field has a label and a description, errors land in a live region so a
screen reader announces them, the result region does the same, and a skip link precedes the
header. Colours come from custom properties with a dark scheme alongside. The layout holds
together at phone width.

Every validation message on screen came from the server. The page writes exactly one of its
own, for when the request never arrived at all.

## Hosting

Azure App Service, F1 free tier, deployed from GitHub Actions.

F1 has no Always On. The site sleeps after roughly twenty minutes of quiet and the next visitor
waits through a cold start. After a deployment the workflow calls `/health` and retries for
three minutes, which wakes the site and proves the deployment actually serves traffic.

`infra/main.bicep` creates the plan and the app, so the environment can be rebuilt from the
repository instead of from somebody's memory of which boxes they ticked in the portal. It
disables Always On, because F1 rejects the deployment otherwise. It turns off FTPS and client
affinity, requires HTTPS and TLS 1.2, and points the health probe at `/health`.

`ci.yml` builds and tests on every push and pull request. `deploy.yml` repeats that, publishes,
and ships. Warnings are errors across the solution, so the analysers run in CI as well as
locally, and a failing test stops the deployment.

## Next steps

`Money` hard-codes 100 cents to the dollar. Yen has no fractional unit at all and the Kuwaiti
dinar has 1000, so that constant belongs on a `Currency` type alongside the symbol and the
words `CurrencyNames` already holds.

A second language would slot in behind `IWholeNumberToWordsConverter`, which is why the
interface exists. A French or German implementation sits beside the English one and neither the
money converter nor the API changes.

The page needs JavaScript. An endpoint accepting a form post would let it work without.

OpenAPI is worth adding once there is more than one endpoint. The README covers it for now,
though this description will drift from the code eventually and a generated document would
not.
