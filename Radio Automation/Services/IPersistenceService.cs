using System.Threading.Tasks;
using Radio_Automation.Models;

namespace Radio_Automation.Services
{
	public interface IPersistenceService
	{
		Task<Playlist> LoadPlaylistAsync(string path);

		Task<bool> SavePlaylistAsync(Playlist p, string path);

		Task<Playlist> ImportZaraPlaylistAsync(string path);

		Task<Playlist> ImportM3UPlaylistAsync(string path);

		Task<Settings> LoadSettingsAsync();

		Task<bool> SaveSettingsAsync(Settings settings);

		Task<EventSchedule> LoadEventScheduleAsync(string path);

		Task<bool> SaveEventScheduleAsync(EventSchedule settings, string path);

	}
}
