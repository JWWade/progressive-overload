(function (global) {
  function clampPositiveInt(value, fallback) {
    var parsed = Number(value);
    if (!Number.isFinite(parsed) || parsed < 1) {
      return fallback;
    }

    return Math.round(parsed);
  }

  function clampNonNegative(value, fallback) {
    var parsed = Number(value);
    if (!Number.isFinite(parsed) || parsed < 0) {
      return fallback;
    }

    return parsed;
  }

  function roundToIncrement(value, increment) {
    if (!Number.isFinite(increment) || increment <= 0) {
      return Number(value.toFixed(2));
    }

    return Number((Math.round(value / increment) * increment).toFixed(2));
  }

  function formatWeight(value, unit) {
    var fixed = Number.isInteger(value) ? String(value) : String(Number(value.toFixed(2)));
    return fixed + ' ' + unit;
  }

  function calculateNextTarget(input) {
    var currentWeight = clampNonNegative(input.currentWeight, 0);
    var currentReps = clampPositiveInt(input.currentReps, 1);
    var currentSets = clampPositiveInt(input.currentSets, 1);
    var strategy = input.strategy || 'weight';
    var increment = clampNonNegative(input.weightIncrement, 2.5);
    var percentIncrease = clampNonNegative(input.percentIncrease, 5);
    var targetReps = clampPositiveInt(input.targetReps, currentReps + 1);
    var unit = input.weightUnit || 'lb';

    var nextWeight = currentWeight;
    var nextReps = currentReps;
    var nextSets = currentSets;
    var summary = 'Repeat the same workout.';

    if (strategy === 'weight') {
      nextWeight = roundToIncrement(currentWeight + increment, increment || 0.5);
      summary = 'Add ' + formatWeight(increment, unit) + ' next session and keep your reps and sets the same.';
    } else if (strategy === 'reps') {
      nextReps = currentReps + 1;
      summary = 'Keep the weight the same and aim for 1 more rep per set next session.';
    } else if (strategy === 'percentage') {
      var rawWeight = currentWeight * (1 + percentIncrease / 100);
      nextWeight = roundToIncrement(rawWeight, increment || 0.5);
      summary = 'Increase the load by ' + Number(percentIncrease.toFixed(2)) + '% and round to your nearest usable jump.';
    } else if (strategy === 'double') {
      if (currentReps >= targetReps) {
        nextWeight = roundToIncrement(currentWeight + increment, increment || 0.5);
        nextReps = Math.max(targetReps - 2, 1);
        summary = 'You hit your rep goal, so move up ' + formatWeight(increment, unit) + ' and restart at ' + nextReps + ' reps.';
      } else {
        nextReps = Math.min(currentReps + 1, targetReps);
        summary = 'Stay at the same weight and build toward ' + targetReps + ' reps before increasing the load.';
      }
    }

    return {
      nextWeight: nextWeight,
      nextReps: nextReps,
      nextSets: nextSets,
      summary: summary,
      unit: unit
    };
  }

  function readForm(form) {
    return {
      currentWeight: form.currentWeight.value,
      currentReps: form.currentReps.value,
      currentSets: form.currentSets.value,
      strategy: form.strategy.value,
      weightIncrement: form.weightIncrement.value,
      percentIncrease: form.percentIncrease.value,
      targetReps: form.targetReps.value,
      weightUnit: form.weightUnit.value
    };
  }

  function updateResult(form, result) {
    document.getElementById('nextWeight').textContent = formatWeight(result.nextWeight, result.unit);
    document.getElementById('nextReps').textContent = String(result.nextReps);
    document.getElementById('nextSets').textContent = String(result.nextSets);
    document.getElementById('strategySummary').textContent = result.summary;

    var strategy = form.strategy.value;
    var percentField = form.percentIncrease.closest('label');
    var targetField = form.targetReps.closest('label');

    percentField.hidden = strategy !== 'percentage';
    targetField.hidden = strategy !== 'double';
  }

  function setupCalculator() {
    if (typeof document === 'undefined') {
      return;
    }

    var form = document.getElementById('calculator-form');
    if (!form) {
      return;
    }

    function render() {
      updateResult(form, calculateNextTarget(readForm(form)));
    }

    form.addEventListener('submit', function (event) {
      event.preventDefault();
      render();
    });

    form.addEventListener('input', render);
    form.addEventListener('change', render);
    render();

    if ('serviceWorker' in navigator) {
      window.addEventListener('load', function () {
        navigator.serviceWorker.register('service-worker.js').catch(function () {
          // Ignore failed registration; the app still works as a static page.
        });
      });
    }
  }

  var api = {
    roundToIncrement: roundToIncrement,
    calculateNextTarget: calculateNextTarget,
    formatWeight: formatWeight
  };

  if (typeof module !== 'undefined' && module.exports) {
    module.exports = api;
  }

  global.ProgressiveOverloadCalculator = api;
  setupCalculator();
})(typeof window !== 'undefined' ? window : globalThis);
