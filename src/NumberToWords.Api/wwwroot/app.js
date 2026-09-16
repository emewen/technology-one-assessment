// Posts to /api/conversions. The reply is either a conversion or an RFC 9457 problem
// document saying why the amount was refused.
(function () {
  'use strict';

  var ENDPOINT = '/api/conversions';

  var form = document.getElementById('converter');
  var input = document.getElementById('amount');
  var submit = document.getElementById('submit');
  var errorPanel = document.getElementById('amount-error');
  var resultPanel = document.getElementById('result');
  var wordsPanel = document.getElementById('words');
  var amountPanel = document.getElementById('parsed-amount');
  var status = document.getElementById('status');
  var examples = document.getElementById('examples');

  function showError(message) {
    resultPanel.hidden = true;
    errorPanel.textContent = message;
    errorPanel.hidden = false;
    input.setAttribute('aria-invalid', 'true');
    status.textContent = message;
  }

  function showResult(conversion) {
    errorPanel.hidden = true;
    errorPanel.textContent = '';
    input.removeAttribute('aria-invalid');

    wordsPanel.textContent = conversion.words;
    amountPanel.textContent = conversion.amount;
    resultPanel.hidden = false;
    status.textContent = conversion.words;
  }

  // Validation messages all come from the server. The two below are for failures that
  // arrive without one: a rate limit, or no response at all.
  function messageFor(response, body) {
    if (body && typeof body.detail === 'string' && body.detail.length > 0) {
      return body.detail;
    }

    if (response.status === 429) {
      return 'Too many requests. Wait a moment and try again.';
    }

    return 'The service could not be reached. Check your connection and try again.';
  }

  function readBody(response) {
    return response.json().catch(function () {
      return null;
    });
  }

  function convert(value) {
    submit.disabled = true;

    return fetch(ENDPOINT, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
      body: JSON.stringify({ value: value })
    })
      .then(function (response) {
        return readBody(response).then(function (body) {
          if (response.ok && body) {
            showResult(body);
          } else {
            showError(messageFor(response, body));
          }
        });
      })
      .catch(function () {
        showError('The service could not be reached. Check your connection and try again.');
      })
      .then(function () {
        submit.disabled = false;
      });
  }

  form.addEventListener('submit', function (event) {
    event.preventDefault();
    convert(input.value);
  });

  // Clear the previous answer on the next keystroke. A stale result under a new figure is
  // worse than an empty panel.
  input.addEventListener('input', function () {
    if (!resultPanel.hidden || !errorPanel.hidden) {
      resultPanel.hidden = true;
      errorPanel.hidden = true;
      input.removeAttribute('aria-invalid');
    }
  });

  examples.addEventListener('click', function (event) {
    var button = event.target.closest('.example');
    if (!button) {
      return;
    }

    input.value = button.getAttribute('data-amount');
    input.focus();
    convert(input.value);
  });
})();
