using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Logging;
using JobToolkit.Core;
using NCrontab;
using Radio_Automation.Models;

namespace Radio_Automation.Events
{
	public sealed class EventScheduler
	{
		private JobManager _jm;
		private JobServer _js;
		private EventSchedule _schedule;
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();
		
		public EventScheduler()
		{
			_jm = JobManager.Default;
			_js = JobServer.Default;
			
			//Event e = new Event(EventType.Time);
			//e.Name = @"Announce Time";
			//e.Demand = Demand.Delayed;
			//e.EndDateTime = DateTime.UtcNow.AddDays(1);
			//e.CronExpression = new CronExpression("*/10 7-17 * * *");
			//var t = new EventTask(e);
			//_jm.Schedule(t, e.CronExpression.GetNextTime(DateTimeOffset.Now),e.CronExpression, new DateTimeOffset(e.EndDateTime), null);
			//Console.Out.WriteLineAsync($"Next time = {e.CronExpression.GetNextTime(DateTimeOffset.Now)}");
		}

		public bool IsRunning { get; private set; }

		/// <summary>
		/// Returns the pending events that are scheduled to occur in the next x minutes
		/// </summary>
		/// <param name="minutes"></param>
		/// <returns></returns>
		public List<PendingEvent> UpNext(int minutes)
		{
			JobDataQueryCriteria c = new JobDataQueryCriteria();
			c.NextScheduleTimeBe = DateTimeOffset.Now;
			c.NextScheduleTimeLe = new DateTimeOffset(DateTime.UtcNow.AddMinutes(minutes));
			c.Status = JobStatus.Scheduled;
			var jobs = _jm.GetAll(c);
			List<PendingEvent> upcomingEvents = new List<PendingEvent>(jobs.Count);
			foreach (var job in jobs)
			{
				//Console.Out.WriteLineAsync($"Job {job.Title} will run {job.NextScheduleTime}");
				if (job.NextScheduleTime.HasValue && job.Task is EventTask t)
				{
					PendingEvent pe = new PendingEvent(t.Event, job.NextScheduleTime.Value.DateTime);
					upcomingEvents.Add(pe);
				}
			}

			return upcomingEvents;
		}

		public void StartServer()
		{
			_js?.Start();
			IsRunning = true;
		}

		public void StopServer()
		{
			if (IsRunning)
			{
				_js?.Stop();
				IsRunning = false;
			}
		}

		public void LoadSchedule(EventSchedule schedule)
		{
			_schedule = schedule;
			StopServer();
			foreach (var job in _jm.GetAll())
			{
				_jm.Dequeue(job.Id);
			}

			foreach (var e in _schedule.Events.Where(x => x.Enabled && x.Trigger == Trigger.Cron))
			{
				try
				{
					var t = new EventTask(e);
					var cronExpression = new CronExpression(e.CronExpression,true);
				
					_jm.Schedule(t, cronExpression.GetNextTime(DateTimeOffset.Now), cronExpression,
						e.Expires?new DateTimeOffset(e.EndDateTime):DateTimeOffset.MaxValue, null);
					
				}
				catch (Exception ex)
				{
					Log.Error($"Error creating event {e.Name} : {ex.Message}");
				}
				
			}
			
			StartServer();
		}
	}
}
