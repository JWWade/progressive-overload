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
		var definitionService = new DefinitionService(repository);
		var selectionService = new ExerciseSelectionService(repository);

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
			System.Console.WriteLine("4. Manage movement/exercise definitions");
			System.Console.WriteLine("5. Recommend next variation by category");
			System.Console.WriteLine("6. Exit");

			var choice = ReadInt("Choice", min: 1, max: 6);
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
					await ShowTrainingSummaryAsync(repository, summaryService);
					break;
				case 4:
					await ManageDefinitionsAsync(definitionService);
					break;
				case 5:
					await RecommendVariationAsync(selectionService);
					break;
				case 6:
					exitRequested = true;
					break;
			}
		}
	}

	private static async Task RecommendVariationAsync(IExerciseSelectionService selectionService)
	{
		var category = ReadRequiredString("Category to evaluate");
		var recommendation = await selectionService.RecommendNextVariationAsync(category);

		if (recommendation is null)
		{
			System.Console.WriteLine("No recommendation available. Add definitions and workout data first.");
			return;
		}

		var lastPerformedText = recommendation.LastPerformedDate.HasValue
			? recommendation.LastPerformedDate.Value.ToString("yyyy-MM-dd")
			: "never";

		System.Console.WriteLine("Recommended next variation:");
		System.Console.WriteLine($"- category: {recommendation.CategoryName}");
		System.Console.WriteLine($"- exercise: {recommendation.ExerciseName}");
		System.Console.WriteLine($"- variation: {recommendation.VariationName}");
		System.Console.WriteLine($"- weighted volume: {recommendation.WeightedVolume}");
		System.Console.WriteLine($"- last performed: {lastPerformedText}");
		System.Console.WriteLine($"- days since last performed: {recommendation.DaysSinceLastPerformed}");
		System.Console.WriteLine($"- rationale: {recommendation.Reason}");
	}

	private static async Task ManageDefinitionsAsync(IDefinitionService definitionService)
	{
		var goBack = false;
		while (!goBack)
		{
			System.Console.WriteLine();
			System.Console.WriteLine("Definition management:");
			System.Console.WriteLine("1. Add or update exercise definition");
			System.Console.WriteLine("2. View catalog");
			System.Console.WriteLine("3. Back");

			var choice = ReadInt("Choice", min: 1, max: 3);
			System.Console.WriteLine();

			switch (choice)
			{
				case 1:
					await AddOrUpdateExerciseDefinitionAsync(definitionService);
					break;
				case 2:
					await ShowCatalogAsync(definitionService);
					break;
				case 3:
					goBack = true;
					break;
			}
		}
	}

	private static async Task AddOrUpdateExerciseDefinitionAsync(IDefinitionService definitionService)
	{
		var categoryName = ReadRequiredString("Category (e.g., push, pull, legs)");
		var exerciseName = ReadRequiredString("Exercise name");
		var exerciseNotes = ReadOptionalString("Exercise notes (optional)");
		var variationCount = ReadInt("Variation count", min: 1, max: 2);

		var variations = new List<ExerciseVariationInput>();
		for (var i = 0; i < variationCount; i++)
		{
			System.Console.WriteLine();
			System.Console.WriteLine($"Variation {i + 1}:");

			var variationName = ReadRequiredString("Variation name");
			var volumeMultiplier = ReadDecimalWithDefault("Volume multiplier", defaultValue: 1m, min: 0.0001m);
			var variationNotes = ReadOptionalString("Variation notes (optional)");

			variations.Add(new ExerciseVariationInput
			{
				Name = variationName,
				VolumeMultiplier = volumeMultiplier,
				Notes = variationNotes
			});
		}

		var result = await definitionService.UpsertExerciseDefinitionAsync(new UpsertExerciseDefinitionRequest
		{
			CategoryName = categoryName,
			ExerciseName = exerciseName,
			Notes = exerciseNotes,
			Variations = variations
		});

		System.Console.WriteLine(result.IsSuccess
			? "Exercise definition saved."
			: $"Could not save exercise definition: {result.ErrorMessage}");
	}

	private static async Task ShowCatalogAsync(IDefinitionService definitionService)
	{
		var catalog = await definitionService.GetCatalogAsync();

		if (catalog.Categories.Count == 0 && catalog.Exercises.Count == 0)
		{
			System.Console.WriteLine("No definitions recorded yet.");
			return;
		}

		if (catalog.Categories.Count > 0)
		{
			System.Console.WriteLine("Categories:");
			foreach (var category in catalog.Categories.OrderBy(c => c.Name))
			{
				System.Console.WriteLine($"- {category.Name}");
			}
		}

		if (catalog.Exercises.Count > 0)
		{
			System.Console.WriteLine();
			System.Console.WriteLine("Exercises:");
			foreach (var exercise in catalog.Exercises.OrderBy(e => e.CategoryName).ThenBy(e => e.Name))
			{
				var exerciseNotes = string.IsNullOrWhiteSpace(exercise.Notes) ? string.Empty : $", notes={exercise.Notes}";
				System.Console.WriteLine($"- {exercise.Name} [{exercise.CategoryName}]{exerciseNotes}");

				foreach (var variation in exercise.Variations)
				{
					var variationNotes = string.IsNullOrWhiteSpace(variation.Notes) ? string.Empty : $", notes={variation.Notes}";
					System.Console.WriteLine($"  - variation: {variation.Name}, volume multiplier={variation.VolumeMultiplier}{variationNotes}");
				}
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
			var variationName = ReadOptionalString("Variation name (optional)");
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
				VariationName = variationName,
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

	private static async Task ShowTrainingSummaryAsync(IProgressiveOverloadRepository repository, IHistorySummaryService summaryService)
	{
		var data = await repository.LoadAsync();
		var summary = await summaryService.GetSummaryAsync();
		var baselines = data.UserProfile?.ExerciseBaselines ?? [];

		if (baselines.Count == 0 && summary.Exercises.Count == 0)
		{
			System.Console.WriteLine("No baseline or workout data recorded yet.");
			return;
		}

		if (baselines.Count > 0)
		{
			System.Console.WriteLine("Current baselines:");
			foreach (var baseline in baselines)
			{
				var notesText = string.IsNullOrWhiteSpace(baseline.Notes) ? string.Empty : $", notes={baseline.Notes}";
				System.Console.WriteLine($"- {baseline.ExerciseName}: {baseline.BaselineWeight} x {baseline.BaselineReps}{notesText}");
			}
		}

		if (summary.Exercises.Count > 0)
		{
			System.Console.WriteLine();
			System.Console.WriteLine("Per-exercise workout summary:");
			foreach (var item in summary.Exercises)
			{
				System.Console.WriteLine(
					$"- {item.ExerciseName}: total sets={item.TotalSets}, total reps={item.TotalReps}, total volume={item.TotalVolume}, last performed={item.LastPerformedDate:yyyy-MM-dd}");
			}
		}
		else
		{
			System.Console.WriteLine();
			System.Console.WriteLine("No workout data recorded yet.");
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

	private static decimal ReadDecimalWithDefault(string label, decimal defaultValue, decimal min)
	{
		while (true)
		{
			System.Console.Write($"{label} (blank for {defaultValue}, min {min}): ");
			var value = System.Console.ReadLine();

			if (string.IsNullOrWhiteSpace(value))
			{
				return defaultValue;
			}

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
