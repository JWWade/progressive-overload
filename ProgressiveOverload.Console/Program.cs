using ProgressiveOverload.Core.Models.Requests;
using ProgressiveOverload.Core.Persistence;
using ProgressiveOverload.Core.Services;

namespace ProgressiveOverload.Console;

public static class Program
{
	public static async Task Main()
	{
		var repository = new JsonFileProgressiveOverloadRepository();
		var profileService = new ProfileService(repository);
		var workoutService = new WorkoutService(repository);
		var summaryService = new HistorySummaryService(repository);

		System.Console.WriteLine("Progressive Overload Prototype");
		System.Console.WriteLine($"Data file: {repository.DataFilePath}");

		var exitRequested = false;
		while (!exitRequested)
		{
			System.Console.WriteLine();
			System.Console.WriteLine("Select an action:");
			System.Console.WriteLine("1. Create or update profile");
			System.Console.WriteLine("2. Log workout session");
			System.Console.WriteLine("3. View training summary");
			System.Console.WriteLine("4. Exit");

			var choice = ReadInt("Choice", min: 1, max: 4);
			System.Console.WriteLine();

			switch (choice)
			{
				case 1:
					await CreateOrUpdateProfileAsync(profileService);
					break;
				case 2:
					await LogWorkoutSessionAsync(workoutService);
					break;
				case 3:
					await ShowTrainingSummaryAsync(summaryService);
					break;
				case 4:
					exitRequested = true;
					break;
			}
		}
	}

	private static async Task CreateOrUpdateProfileAsync(IProfileService profileService)
	{
		var name = ReadRequiredString("Name");
		var baselineCount = ReadInt("How many exercise baselines to record", min: 0, max: 100);

		var baselines = new List<ExerciseBaselineInput>();
		for (var i = 0; i < baselineCount; i++)
		{
			System.Console.WriteLine();
			System.Console.WriteLine($"Baseline {i + 1}:");

			var exerciseName = ReadRequiredString("Exercise name");
			var weight = ReadDecimal("Baseline weight", min: 0);
			var reps = ReadInt("Baseline reps", min: 1, max: 1000);
			var notes = ReadOptionalString("Notes (optional)");

			baselines.Add(new ExerciseBaselineInput
			{
				ExerciseName = exerciseName,
				BaselineWeight = weight,
				BaselineReps = reps,
				Notes = notes
			});
		}

		var result = await profileService.UpsertProfileAsync(new ProfileUpsertRequest
		{
			Name = name,
			ExerciseBaselines = baselines
		});

		System.Console.WriteLine(result.IsSuccess
			? "Profile saved."
			: $"Could not save profile: {result.ErrorMessage}");
	}

	private static async Task LogWorkoutSessionAsync(IWorkoutService workoutService)
	{
		var date = ReadDate("Session date (yyyy-MM-dd, blank for today)");
		var exerciseCount = ReadInt("How many exercises in this session", min: 1, max: 100);
		var exercises = new List<ExerciseDraft>();

		for (var i = 0; i < exerciseCount; i++)
		{
			System.Console.WriteLine();
			System.Console.WriteLine($"Exercise {i + 1}:");

			var exerciseName = ReadRequiredString("Exercise name");
			var setCount = ReadInt("Number of sets", min: 1, max: 100);
			var sets = new List<SetDraft>();

			for (var setIndex = 0; setIndex < setCount; setIndex++)
			{
				var reps = ReadInt($"Set {setIndex + 1} reps", min: 1, max: 1000);
				var weight = ReadDecimal($"Set {setIndex + 1} weight", min: 0);

				sets.Add(new SetDraft
				{
					Reps = reps,
					Weight = weight
				});
			}

			var exerciseNotes = ReadOptionalString("Exercise notes (optional)");

			exercises.Add(new ExerciseDraft
			{
				ExerciseName = exerciseName,
				Notes = exerciseNotes,
				Sets = sets
			});
		}

		var sessionNotes = ReadOptionalString("Session notes (optional)");
		var result = await workoutService.LogWorkoutAsync(new WorkoutSessionDraft
		{
			SessionDate = date,
			Notes = sessionNotes,
			Exercises = exercises
		});

		System.Console.WriteLine(result.IsSuccess
			? "Workout session saved."
			: $"Could not save workout session: {result.ErrorMessage}");
	}

	private static async Task ShowTrainingSummaryAsync(IHistorySummaryService summaryService)
	{
		var summary = await summaryService.GetSummaryAsync();

		if (summary.Exercises.Count == 0)
		{
			System.Console.WriteLine("No workout data recorded yet.");
			return;
		}

		System.Console.WriteLine("Per-exercise summary:");
		foreach (var item in summary.Exercises)
		{
			System.Console.WriteLine(
				$"- {item.ExerciseName}: total sets={item.TotalSets}, total reps={item.TotalReps}, total volume={item.TotalVolume}, last performed={item.LastPerformedDate:yyyy-MM-dd}");
		}
	}

	private static string ReadRequiredString(string label)
	{
		while (true)
		{
			System.Console.Write($"{label}: ");
			var value = System.Console.ReadLine();
			if (!string.IsNullOrWhiteSpace(value))
			{
				return value.Trim();
			}

			System.Console.WriteLine("A value is required.");
		}
	}

	private static string? ReadOptionalString(string label)
	{
		System.Console.Write($"{label}: ");
		var value = System.Console.ReadLine();
		return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
	}

	private static int ReadInt(string label, int min, int max)
	{
		while (true)
		{
			System.Console.Write($"{label} [{min}-{max}]: ");
			var value = System.Console.ReadLine();
			if (int.TryParse(value, out var parsed) && parsed >= min && parsed <= max)
			{
				return parsed;
			}

			System.Console.WriteLine("Enter a valid number in range.");
		}
	}

	private static decimal ReadDecimal(string label, decimal min)
	{
		while (true)
		{
			System.Console.Write($"{label} (>= {min}): ");
			var value = System.Console.ReadLine();
			if (decimal.TryParse(value, out var parsed) && parsed >= min)
			{
				return parsed;
			}

			System.Console.WriteLine("Enter a valid decimal value.");
		}
	}

	private static DateOnly ReadDate(string label)
	{
		while (true)
		{
			System.Console.Write($"{label}: ");
			var value = System.Console.ReadLine();

			if (string.IsNullOrWhiteSpace(value))
			{
				return DateOnly.FromDateTime(DateTime.Today);
			}

			if (DateOnly.TryParse(value, out var parsed))
			{
				return parsed;
			}

			System.Console.WriteLine("Enter a valid date, for example 2026-08-23.");
		}
	}
}
