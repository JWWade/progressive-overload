const test = require('node:test');
const assert = require('node:assert/strict');
const { calculateNextTarget, roundToIncrement } = require('./app.js');

test('weight strategy adds configured increment', () => {
  const result = calculateNextTarget({
    currentWeight: 100,
    currentReps: 8,
    currentSets: 3,
    strategy: 'weight',
    weightIncrement: 2.5,
    percentIncrease: 5,
    targetReps: 10,
    weightUnit: 'lb'
  });

  assert.equal(result.nextWeight, 102.5);
  assert.equal(result.nextReps, 8);
  assert.equal(result.nextSets, 3);
});

test('percentage strategy rounds to nearest increment', () => {
  const result = calculateNextTarget({
    currentWeight: 95,
    currentReps: 8,
    currentSets: 3,
    strategy: 'percentage',
    weightIncrement: 2.5,
    percentIncrease: 5,
    targetReps: 10,
    weightUnit: 'lb'
  });

  assert.equal(result.nextWeight, 100);
});

test('double progression adds reps until goal is reached', () => {
  const result = calculateNextTarget({
    currentWeight: 100,
    currentReps: 8,
    currentSets: 3,
    strategy: 'double',
    weightIncrement: 2.5,
    percentIncrease: 5,
    targetReps: 10,
    weightUnit: 'lb'
  });

  assert.equal(result.nextWeight, 100);
  assert.equal(result.nextReps, 9);
});

test('double progression increases load after hitting rep goal', () => {
  const result = calculateNextTarget({
    currentWeight: 100,
    currentReps: 10,
    currentSets: 3,
    strategy: 'double',
    weightIncrement: 5,
    percentIncrease: 5,
    targetReps: 10,
    weightUnit: 'lb'
  });

  assert.equal(result.nextWeight, 105);
  assert.equal(result.nextReps, 8);
});

test('roundToIncrement handles decimal jumps', () => {
  assert.equal(roundToIncrement(51.1, 0.5), 51);
  assert.equal(roundToIncrement(51.3, 0.5), 51.5);
});
