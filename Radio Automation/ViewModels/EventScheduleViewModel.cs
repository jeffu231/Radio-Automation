﻿using System;
using System.Threading.Tasks;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using JobToolkit.Core;
using Radio_Automation.Models;
using Radio_Automation.Services;

namespace Radio_Automation.ViewModels
{
	public class EventScheduleViewModel:ViewModelBase
	{
		public EventScheduleViewModel(EventSchedule schedule)
		{
			EventSchedule = schedule;
		}

		#region Overrides of ViewModelBase

		/// <inheritdoc />
		public override string Title => "Event Schedule";

		#endregion

		#region Name model property

		/// <summary>
		/// Gets or sets the Name value.
		/// </summary>
		[ViewModelToModel("EventSchedule", "Name")]
		public string Name
		{
			get { return GetValue<string>(NameProperty); }
			private set { SetValue(NameProperty, value); }
		}

		/// <summary>
		/// Name property data.
		/// </summary>
		public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string));

		#endregion

		#region EventSchedule model property

		/// <summary>
		/// Gets or sets the EventSchedule value.
		/// </summary>
		[Model]
		public EventSchedule EventSchedule
		{
			get { return GetValue<EventSchedule>(EventScheduleProperty); }
			private set { SetValue(EventScheduleProperty, value); }
		}

		/// <summary>
		/// EventSchedule property data.
		/// </summary>
		public static readonly PropertyData EventScheduleProperty = RegisterProperty("EventSchedule", typeof(EventSchedule));

		#endregion

		#region Path property

		/// <summary>
		/// Gets or sets the Path value.
		/// </summary>
		public string Path
		{
			get { return GetValue<string>(PathProperty); }
			set { SetValue(PathProperty, value); }
		}

		/// <summary>
		/// Path property data.
		/// </summary>
		public static readonly PropertyData PathProperty = RegisterProperty("Path", typeof(string));

		#endregion

		#region SelectedItem property

		/// <summary>
		/// Gets or sets the SelectedItem value.
		/// </summary>
		public Event SelectedItem
		{
			get { return GetValue<Event>(SelectedItemProperty); }
			set
			{
				SetValue(SelectedItemProperty, value);
				UpdateCommandStates();
			}
		}

		/// <summary>
		/// SelectedItem property data.
		/// </summary>
		public static readonly PropertyData SelectedItemProperty = RegisterProperty("SelectedItem", typeof(Event));

		#endregion

		#region AddEvent command

		private TaskCommand _addEventCommand;

		/// <summary>
		/// Gets the AddEvent command.
		/// </summary>
		public TaskCommand AddEventCommand
		{
			get { return _addEventCommand ?? (_addEventCommand = new TaskCommand(AddEventAsync)); }
		}

		/// <summary>
		/// Method to invoke when the AddEvent command is executed.
		/// </summary>
		private async Task AddEventAsync()
		{
			Event e = new Event(EventType.Time);
			e.Name = @"Announce Time";
			e.Enabled = true;
			e.Demand = Demand.Delayed;
			e.EndDateTime = DateTime.UtcNow.AddDays(1);
			e.CronExpression = new CronExpression("*/10 7-17 * * *");
			EventSchedule.Events.Add(e);
			await Task.CompletedTask;
		}

		#endregion

		#region DeleteEvent command

		private TaskCommand _deleteEventCommand;

		/// <summary>
		/// Gets the DeleteEvent command.
		/// </summary>
		public TaskCommand DeleteEventCommand
		{
			get { return _deleteEventCommand ?? (_deleteEventCommand = new TaskCommand(DeleteEventAsync, CanDeleteEvent)); }
		}

		/// <summary>
		/// Method to invoke when the DeleteEvent command is executed.
		/// </summary>
		private async Task DeleteEventAsync()
		{
			EventSchedule.Events.Remove(SelectedItem);
			await Task.CompletedTask;
		}

		/// <summary>
		/// Method to check whether the DeleteEvent command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanDeleteEvent()
		{
			return SelectedItem != null;
		}

		#endregion

		#region EditEvent command

		private TaskCommand _editEventCommand;

		/// <summary>
		/// Gets the EditEvent command.
		/// </summary>
		public TaskCommand EditEventCommand
		{
			get { return _editEventCommand ?? (_editEventCommand = new TaskCommand(EditEvent, CanEditEvent)); }
		}

		/// <summary>
		/// Method to invoke when the EditEvent command is executed.
		/// </summary>
		private async Task EditEvent()
		{
			EditEventViewModel viewModel = new EditEventViewModel(SelectedItem);
			var dependencyResolver = this.GetDependencyResolver();
			var uiVisualizerService = dependencyResolver.Resolve<IUIVisualizerService>();
			await uiVisualizerService.ShowDialogAsync(viewModel);
		}

		/// <summary>
		/// Method to check whether the EditEvent command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanEditEvent()
		{
			return SelectedItem != null;
		}

		#endregion


		#region SaveSchedule command

		private TaskCommand _saveScheduleCommand;

		/// <summary>
		/// Gets the SaveSchedule command.
		/// </summary>
		public TaskCommand SaveScheduleCommand
		{
			get { return _saveScheduleCommand ?? (_saveScheduleCommand = new TaskCommand(SaveScheduleAsync)); }
		}

		/// <summary>
		/// Method to invoke when the SaveSchedule command is executed.
		/// </summary>
		private async Task<bool> SaveScheduleAsync()
		{
			var dependencyResolver = this.GetDependencyResolver();
			var saveFileService = dependencyResolver.Resolve<ISaveFileService>();
			var pleaseWaitService = dependencyResolver.Resolve<IPleaseWaitService>();
			var persistenceService = dependencyResolver.Resolve<IPersistenceService>();
			if (string.IsNullOrEmpty(Path))
			{
				saveFileService.Filter = "Event Schedule|*.evs";
				saveFileService.Title = @"Save Event Schedule";
				
				if (await saveFileService.DetermineFileAsync())
				{
					Path = saveFileService.FileName;
				}
			}

			pleaseWaitService.Show();
			var success = await persistenceService.SaveEventScheduleAsync(EventSchedule, Path);
			pleaseWaitService.Hide();
			
			return success;
		}

		#endregion

		#region OpenSchedule command

		private TaskCommand _openScheduleCommand;

		/// <summary>
		/// Gets the OpenSchedule command.
		/// </summary>
		public TaskCommand OpenScheduleCommand
		{
			get { return _openScheduleCommand ?? (_openScheduleCommand = new TaskCommand(OpenScheduleAsync)); }
		}

		/// <summary>
		/// Method to invoke when the OpenSchedule command is executed.
		/// </summary>
		private async Task OpenScheduleAsync()
		{
			var dependencyResolver = this.GetDependencyResolver();
			var openFileService = dependencyResolver.Resolve<IOpenFileService>();
			var pleaseWaitService = dependencyResolver.Resolve<IPleaseWaitService>();
			var persistenceService = dependencyResolver.Resolve<IPersistenceService>();
			openFileService.Filter = "Event Schedule|*.evs";
			openFileService.Title = @"Open Event Schedule";
			if (await openFileService.DetermineFileAsync())
			{
				pleaseWaitService.Show();
				EventSchedule = await persistenceService.LoadEventScheduleAsync(openFileService.FileName);
				Path = openFileService.FileName;
				pleaseWaitService.Hide();
			}
		}

		#endregion

		#region NewSchedule command

		private TaskCommand _newScheduleCommand;

		/// <summary>
		/// Gets the NewSchedule command.
		/// </summary>
		public TaskCommand NewScheduleCommand
		{
			get { return _newScheduleCommand ?? (_newScheduleCommand = new TaskCommand(NewScheduleAsync)); }
		}

		/// <summary>
		/// Method to invoke when the NewSchedule command is executed.
		/// </summary>
		private async Task NewScheduleAsync()
		{
			EventSchedule = new EventSchedule();
			Path = string.Empty;
			await TaskHelper.Completed;
		}

		#endregion

		#region Ok command

		private TaskCommand _okCommand;

		/// <summary>
		/// Gets the Ok command.
		/// </summary>
		public TaskCommand OkCommand
		{
			get { return _okCommand ?? (_okCommand = new TaskCommand(OkAsync)); }
		}

		/// <summary>
		/// Method to invoke when the Ok command is executed.
		/// </summary>
		private async Task OkAsync()
		{
			await this.SaveAndCloseViewModelAsync();
		}

		#endregion


		#region Overrides of ViewModelBase

		/// <inheritdoc />
		protected override async Task<bool> SaveAsync()
		{
			return await SaveScheduleAsync();
		}

		#endregion

		#region Overrides of ViewModelBase

		/// <inheritdoc />
		protected override Task<bool> CancelAsync()
		{
			Console.Out.WriteLineAsync("Cancel!");
			return base.CancelAsync();
		}

		#endregion

		protected void UpdateCommandStates()
		{
			var viewModelBase = this as ViewModelBase;
			var commandManager = viewModelBase.GetViewModelCommandManager();
			commandManager.InvalidateCommands();
		}
	}
}
