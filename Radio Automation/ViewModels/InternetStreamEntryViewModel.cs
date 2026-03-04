using Catel.Data;
using Catel.MVVM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Radio_Automation.ViewModels
{
	internal class InternetStreamEntryViewModel : ViewModelBase
	{
		public InternetStreamEntryViewModel()
		{
			Title = "Add Internet Stream";
			StreamName = @"New Internet Stream";
			StreamUrl = @"http://example.com/stream";
			DeferValidationUntilFirstSaveCall = false;
		}

		#region StreamName property

		/// <summary>
		/// Gets or sets the StreamName value.
		/// </summary>
		public string StreamName 
		{
			get { return GetValue<string>(StreamNameProperty); }
			set { SetValue(StreamNameProperty, value); }
		} 

		/// <summary>
		/// StreamName property data.
		/// </summary>
		public static readonly IPropertyData StreamNameProperty = RegisterProperty<string>(nameof(StreamName));

		#endregion

		#region StreamUrl property

		/// <summary>
		/// Gets or sets the StreamUrl value.
		/// </summary>
		public string StreamUrl
		{
			get { return GetValue<string>(StreamUrlProperty); }
			set { SetValue(StreamUrlProperty, value); }
		}

		/// <summary>
		/// StreamUrl property data.
		/// </summary>
		public static readonly IPropertyData StreamUrlProperty = RegisterProperty<string>(nameof(StreamUrl));

		#endregion


		#region Ok command

		private TaskCommand _okCommand;

		/// <summary>
		/// Gets the Ok command.
		/// </summary>
		public TaskCommand OkCommand
		{
			get { return _okCommand ??= new TaskCommand(OkAsync, CanOk); }
		}

		/// <summary>
		/// Method to invoke when the Ok command is executed.
		/// </summary>
		private async Task OkAsync()
		{
			//await SaveViewModelAsync();
			await CloseViewModelAsync(true);
		}

		/// <summary>
		/// Method to check whether the Ok command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanOk()
		{
			return true;
		}

		#endregion

		/// <summary>
		/// Validates the field values of this object. Override this method to enable
		/// validation of field values.
		/// </summary>
		/// <param name="validationResults">The validation results, add additional results to this list.</param>
		protected override void ValidateFields(List<IFieldValidationResult> validationResults)
		{
			if (string.IsNullOrEmpty(StreamName))
			{
				validationResults.Add(
					FieldValidationResult.CreateError(StreamNameProperty, "Stream name cannot be empty"));
			}

			if (string.IsNullOrEmpty(StreamUrl))
			{
				validationResults.Add(
					FieldValidationResult.CreateError(StreamUrlProperty, "Stream URL cannot be empty"));
			}

			if (!IsValidUrl(StreamUrl))
			{
				validationResults.Add(
					FieldValidationResult.CreateError(StreamUrlProperty, "Stream URL is not a valid URL"));
			}
		}

		private static bool IsValidUrl(string urlString)
		{
			if (string.IsNullOrWhiteSpace(urlString))
			{
				return false;
			}

			bool isValid = Uri.TryCreate(urlString, UriKind.Absolute, out var uriResult);
			
			if (isValid && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
			{
				return true;
			}

			return false;
		}

	}
}
