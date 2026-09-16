# Number to Words

A web page that converts an amount of money into words.

```
Input:  123.45
Output: ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS
```

The conversion runs on the server in C# and returns a string. The page posts the amount to
`POST /api/conversions` and displays the answer.

Built for the TechnologyOne developer technical test.

## Requirements

[.NET SDK 10.0](https://dotnet.microsoft.com/download) or later. `global.json` pins the major
version, so any 10.0.x SDK works and an older one fails with a clear message.

Nothing else. Outside the test projects the solution references no NuGet package.

The solution file uses the XML `.slnx` format. The `dotnet` CLI, Visual Studio 2022 17.14 and
later, and Rider all open it. Building from the command line needs only the SDK.

```bash
dotnet --list-sdks
```

## Running it

```bash
git clone https://github.com/emewen/technology-one-assessment.git
cd technology-one-assessment
dotnet run --project src/NumberToWords.Api
```

Open the URL the console prints, usually <http://localhost:5xxx>.

## Running the tests

```bash
dotnet test
```

42 tests in a couple of seconds. 31 cover the domain, 11 drive the application over HTTP.

With a report and coverage:

```bash
dotnet test --logger "trx;LogFileName=results.trx" --collect:"XPlat Code Coverage"
```

## Building for release

```bash
dotnet build --configuration Release
dotnet publish src/NumberToWords.Api/NumberToWords.Api.csproj --configuration Release --output ./publish
```

Warnings are errors across the solution. A build that succeeds has also cleared every analyser
at `AnalysisMode=All`.

## Using the page

Type an amount and select **Convert**, or press Enter. The words appear below the field
alongside the amount the service read, so you can confirm it understood you.

Six examples sit under the form. Selecting one fills the field and converts it.

If the service refuses an amount, the reason appears under the field in the wording the server
sent.

## Using the API

### Converting an amount

```bash
curl -X POST http://localhost:5000/api/conversions \
  -H "Content-Type: application/json" \
  -d '{"value":"123.45"}'
```

```json
{
  "input": "123.45",
  "amount": "123.45",
  "words": "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS"
}
```

`input` is what you sent. `amount` is what the parser read, worth checking after you send
something like `1,234.5`.

### A refused amount

```bash
curl -X POST http://localhost:5000/api/conversions \
  -H "Content-Type: application/json" \
  -d '{"value":"1.234"}'
```

```json
{
  "title": "The amount could not be read.",
  "status": 400,
  "detail": "Enter at most two decimal places. Amounts are not rounded for you.",
  "code": "input.too_many_decimal_places"
}
```

The `code` stays stable across releases. Branch on it, never on `detail`.

| Code | Cause |
| --- | --- |
| `input.empty` | Nothing to convert. |
| `input.too_long` | More than 64 characters. |
| `input.invalid_character` | A character that cannot appear in an amount. |
| `input.multiple_decimal_points` | More than one decimal point. |
| `input.too_many_decimal_places` | More than two decimal places. |
| `input.no_digits` | Punctuation or a sign, but no digits. |
| `value.too_large` | More than 30 digits before the decimal point. |
| `request.malformed` | The body is not JSON this endpoint can read. |

### Health

```bash
curl http://localhost:5000/health
```

Returns `Healthy`. App Service uses it as a probe and the deployment workflow waits on it.

## What the converter accepts

| Rule | Detail |
| --- | --- |
| Decimal places | Two at most. `1.234` is refused, never rounded, so your figure does not change without you seeing it. |
| Size | Up to 30 digits before the decimal point, reaching the octillions. |
| Formatting | The parser ignores spaces, commas and a leading dollar sign, so `$1,234.56` works. |
| Sign | A leading minus gives an amount below zero, reading as `MINUS ...`. |
| Wording | Australian usage. `123` is `ONE HUNDRED AND TWENTY-THREE` and `1001` is `ONE THOUSAND AND ONE`. |

An empty part disappears from the answer. `0.45` gives `FORTY-FIVE CENTS` and `5.00` gives
`FIVE DOLLARS`. Zero gives `ZERO DOLLARS`.

## Layout

```
src/
  NumberToWords.Domain/     The algorithm. No dependencies at all.
    Conversion/             Number naming and currency wording.
    Parsing/                Reading text into an amount.
    Money.cs                The value object.
    Result.cs               Success or a named failure.
  NumberToWords.Api/        Minimal API and the page.
    Contracts/              Request and response records.
    Endpoints/              Route mapping and the handler.
    Infrastructure/         DI, security headers, exception handling, logging.
    wwwroot/                index.html, styles.css, app.js.
tests/
  NumberToWords.Domain.Tests/
  NumberToWords.Api.Tests/
infra/
  main.bicep                App Service plan and web app.
docs/
  design.md                 The approach, and the alternatives rejected.
  test-plan.md              Testing notes and the gaps.
```

## Hosting it on Azure

The site runs on App Service under the F1 free tier. Set it up once and every push deploys
from then on.

### 1. Create the infrastructure

```bash
az login
az group create --name number-to-words-rg --location australiaeast

az deployment group create \
  --resource-group number-to-words-rg \
  --template-file infra/main.bicep \
  --parameters webAppName=<your-unique-name> location=australiaeast
```

The name has to be free across `azurewebsites.net`. The deployment prints the site URL when it
finishes.

The portal works too, as long as the plan is F1 Linux and the stack is .NET 10. Leave Always On
off. F1 does not offer it and the deployment fails if you ask for it.

### 2. Give GitHub the credentials

Download the publish profile:

```bash
az webapp deployment list-publishing-profiles \
  --resource-group number-to-words-rg \
  --name <your-unique-name> \
  --xml
```

In the repository, under **Settings**, open **Secrets and variables**, then **Actions**:

- On the **Secrets** tab, add `AZURE_WEBAPP_PUBLISH_PROFILE` holding that XML.
- On the **Variables** tab, add `AZURE_WEBAPP_NAME` holding the app name.

The publish profile contains a deployment password. Keep it in the secret and out of the
repository.

### 3. Push

`deploy.yml` runs on every push to `main`. It builds, tests, publishes and deploys, then polls
`/health` for three minutes and fails the run if the site never answers.

The deploy job skips while `AZURE_WEBAPP_NAME` is unset, so a fresh clone still gets a green
build.

### Free tier limits

There is no Always On, so the site sleeps after about twenty minutes of quiet and the next
visitor waits several seconds while it wakes. The deployment workflow wakes it, which keeps the
first visit after a deployment quick.

A daily compute quota applies, and exceeding it stops the site until the quota resets. The rate
limiter caps each client at 120 requests a minute to keep one caller from spending it.

F1 offers no custom domain certificate, though the `azurewebsites.net` host name still serves
over HTTPS. Scaling past one instance needs B1 or higher.

## Documents

- [Design document](docs/design.md). The approach, why I chose it, and what I rejected.
- [Testing notes](docs/test-plan.md). What is covered, what I checked by hand, and the gaps.
