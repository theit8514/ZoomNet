using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Http.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZoomNet.Models;
using ZoomNet.Utilities;

namespace ZoomNet.Resources
{
	/// <summary>
	/// Allows you to manage webinars.
	/// </summary>
	/// <seealso cref="ZoomNet.Resources.IWebinars" />
	/// <remarks>
	/// See <a href="https://marketplace.zoom.us/docs/api-reference/zoom-api/webinars/webinars">Zoom documentation</a> for more information.
	/// </remarks>
	public class Webinars : IWebinars
	{
		private readonly Pathoschild.Http.Client.IClient _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="Webinars" /> class.
		/// </summary>
		/// <param name="client">The HTTP client.</param>
		internal Webinars(Pathoschild.Http.Client.IClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Retrieve all webinars for a user.
		/// </summary>
		/// <param name="userId">The user Id or email address.</param>
		/// <param name="recordsPerPage">The number of records returned within a single API call.</param>
		/// <param name="page">The current page number of returned records.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An array of <see cref="Webinar" />.
		/// </returns>
		public Task<PaginatedResponse<Webinar>> GetAllAsync(string userId, int recordsPerPage = 30, int page = 1, CancellationToken cancellationToken = default)
		{
			if (recordsPerPage < 1 || recordsPerPage > 300)
			{
				throw new ArgumentOutOfRangeException(nameof(recordsPerPage), "Records per page must be between 1 and 300");
			}

			return _client
				.GetAsync($"users/{userId}/webinars")
				.WithArgument("page_size", recordsPerPage)
				.WithArgument("page", page)
				.WithCancellationToken(cancellationToken)
				.AsPaginatedResponse<Webinar>("webinars", new WebinarConverter());
		}

		/// <summary>
		/// Creates a scheduled webinar for a user.
		/// </summary>
		/// <param name="userId">The user Id or email address.</param>
		/// <param name="topic">Webinar topic.</param>
		/// <param name="agenda">Webinar description.</param>
		/// <param name="start">Webinar start time.</param>
		/// <param name="duration">Webinar duration (minutes).</param>
		/// <param name="password">Password to join the webinar. Password may only contain the following characters: [a-z A-Z 0-9 @ - _ *]. Max of 10 characters.</param>
		/// <param name="settings">Webinar settings.</param>
		/// <param name="trackingFields">Tracking fields.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The new webinar.
		/// </returns>
		/// <exception cref="System.Exception">Thrown when an exception occured while creating the webinar.</exception>
		public Task<Webinar> CreateScheduledWebinarAsync(string userId, string topic, string agenda, DateTime start, int duration, string password = null, WebinarSettings settings = null, IDictionary<string, string> trackingFields = null, CancellationToken cancellationToken = default)
		{
			var data = new JObject()
			{
				{ "type", 5 }
			};
			data.AddPropertyIfValue("topic", topic);
			data.AddPropertyIfValue("agenda", agenda);
			data.AddPropertyIfValue("password", password);
			data.AddPropertyIfValue("start_time", start.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"));
			data.AddPropertyIfValue("duration", duration);
			data.AddPropertyIfValue("timezone", "UTC");
			data.AddPropertyIfValue("settings", settings);
			data.AddPropertyIfValue("tracking_fields", trackingFields?.Select(tf => new JObject() { { "field", tf.Key }, { "value", tf.Value } }));

			return _client
				.PostAsync($"users/{userId}/webinars")
				.WithJsonBody(data)
				.WithCancellationToken(cancellationToken)
				.AsObject<Webinar>();
		}

		/// <summary>
		/// Creates a recurring webinar for a user.
		/// </summary>
		/// <param name="userId">The user Id or email address.</param>
		/// <param name="topic">Webinar topic.</param>
		/// <param name="agenda">Webinar description.</param>
		/// <param name="start">Webinar start time.</param>
		/// <param name="duration">Webinar duration (minutes).</param>
		/// <param name="recurrence">Recurrence information.</param>
		/// <param name="password">Password to join the webinar. Password may only contain the following characters: [a-z A-Z 0-9 @ - _ *]. Max of 10 characters.</param>
		/// <param name="settings">Webinar settings.</param>
		/// <param name="trackingFields">Tracking fields.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The new webinar.
		/// </returns>
		/// <exception cref="System.Exception">Thrown when an exception occured while creating the webinar.</exception>
		public Task<RecurringWebinar> CreateRecurringWebinarAsync(string userId, string topic, string agenda, DateTime? start, int duration, RecurrenceInfo recurrence, string password = null, WebinarSettings settings = null, IDictionary<string, string> trackingFields = null, CancellationToken cancellationToken = default)
		{
			var data = new JObject()
			{
				// 6 = Recurring with no fixed time
				// 9 = Recurring with fixed time
				{ "type", start.HasValue ? 9 : 6 }
			};
			data.AddPropertyIfValue("topic", topic);
			data.AddPropertyIfValue("agenda", agenda);
			data.AddPropertyIfValue("password", password);
			data.AddPropertyIfValue("start_time", start?.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"));
			data.AddPropertyIfValue("duration", duration);
			data.AddPropertyIfValue("recurrence", recurrence);
			data.AddPropertyIfValue("timezone", "UTC");
			data.AddPropertyIfValue("settings", settings);
			data.AddPropertyIfValue("tracking_fields", trackingFields?.Select(tf => new JObject() { { "field", tf.Key }, { "value", tf.Value } }));

			return _client
				.PostAsync($"users/{userId}/webinars")
				.WithJsonBody(data)
				.WithCancellationToken(cancellationToken)
				.AsObject<RecurringWebinar>();
		}

		/// <summary>
		/// Retrieve the details of a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="occurrenceId">The webinar occurrence id.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The <see cref="Webinar" />.
		/// </returns>
		public Task<Webinar> GetAsync(long webinarId, string occurrenceId = null, CancellationToken cancellationToken = default)
		{
			return _client
				.GetAsync($"webinars/{webinarId}")
				.WithArgument("occurrence_id", occurrenceId)
				.WithCancellationToken(cancellationToken)
				.AsObject<Webinar>(null, new WebinarConverter());
		}

		/// <summary>
		/// Delete a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="occurrenceId">The webinar occurrence id.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task DeleteAsync(long webinarId, string occurrenceId = null, CancellationToken cancellationToken = default)
		{
			return _client
				.DeleteAsync($"webinars/{webinarId}")
				.WithArgument("occurrence_id", occurrenceId)
				.WithCancellationToken(cancellationToken)
				.AsMessage();
		}

		/// <summary>
		/// End a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task EndAsync(long webinarId, CancellationToken cancellationToken = default)
		{
			var data = new JObject()
			{
				{ "action", "end" }
			};

			return _client
				.PutAsync($"webinars/{webinarId}/status")
				.WithJsonBody(data)
				.WithCancellationToken(cancellationToken)
				.AsMessage();
		}

		/// <summary>
		/// List panelists of a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An array of <see cref="Panelist"/>.
		/// </returns>
		public Task<Panelist[]> GetPanelistsAsync(long webinarId, CancellationToken cancellationToken = default)
		{
			return _client
				.GetAsync($"webinars/{webinarId}/panelists")
				.WithCancellationToken(cancellationToken)
				.AsObject<Panelist[]>("panelists");
		}

		/// <summary>
		/// Add a single panelist to a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="email">Panelist's email address.</param>
		/// <param name="fullName">Panelist's full name.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The unique identifier of the new panelist.
		/// </returns>
		public async Task<Panelist> AddPanelistAsync(long webinarId, string email, string fullName, CancellationToken cancellationToken = default)
		{
			var panelists = await AddPanelistsAsync(webinarId, new (string Email, string FullName)[] { (email, fullName) }, cancellationToken).ConfigureAwait(false);
			return panelists.FirstOrDefault();
		}

		/// <summary>
		/// Add multiple panelists to a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="panelists">The panelists to add to the webinar.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task<Panelist[]> AddPanelistsAsync(long webinarId, IEnumerable<(string Email, string FullName)> panelists, CancellationToken cancellationToken = default)
		{
			var data = new JObject();
			data.AddPropertyIfValue("panelists", panelists.Select(p => new { email = p.Email, name = p.FullName }).ToArray());

			return _client
				.PostAsync($"webinars/{webinarId}/panelists")
				.WithJsonBody(data)
				.WithCancellationToken(cancellationToken)
				.AsObject<Panelist[]>();
		}

		/// <summary>
		/// Remove a single panelist from a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="panelistId">Panelist's email address.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task RemovePanelistAsync(long webinarId, string panelistId, CancellationToken cancellationToken = default)
		{
			return _client
				.DeleteAsync($"webinars/{webinarId}/panelists/{panelistId}")
				.WithCancellationToken(cancellationToken)
				.AsMessage();
		}

		/// <summary>
		/// Remove all panelists from a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task RemoveAllPanelistsAsync(long webinarId, CancellationToken cancellationToken = default)
		{
			return _client
				.DeleteAsync($"webinars/{webinarId}/panelists")
				.WithCancellationToken(cancellationToken)
				.AsMessage();
		}

		/// <summary>
		/// List the users that have registered for a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="status">The registrant status.</param>
		/// <param name="occurrenceId">The webinar occurrence id.</param>
		/// <param name="recordsPerPage">The number of records returned within a single API call.</param>
		/// <param name="page">The current page number of returned records.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An array of <see cref="Registrant" />.
		/// </returns>
		public Task<PaginatedResponse<Registrant>> GetRegistrantsAsync(long webinarId, RegistrantStatus status, string occurrenceId = null, int recordsPerPage = 30, int page = 1, CancellationToken cancellationToken = default)
		{
			if (recordsPerPage < 1 || recordsPerPage > 300)
			{
				throw new ArgumentOutOfRangeException(nameof(recordsPerPage), "Records per page must be between 1 and 300");
			}

			return _client
				.GetAsync($"webinars/{webinarId}/registrants")
				.WithArgument("status", JToken.Parse(JsonConvert.SerializeObject(status)).ToString())
				.WithArgument("occurrence_id", occurrenceId)
				.WithArgument("page_size", recordsPerPage)
				.WithArgument("page", page)
				.WithCancellationToken(cancellationToken)
				.AsPaginatedResponse<Registrant>("registrants");
		}

		/// <summary>
		/// Add a registrant to a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="email">A valid email address.</param>
		/// <param name="firstName">User's first name.</param>
		/// <param name="lastName">User's last name.</param>
		/// <param name="occurrenceId">The webinar occurrence id.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A <see cref="Registrant" />.
		/// </returns>
		public Task<Registrant> AddRegistrantAsync(long webinarId, string email, string firstName, string lastName, string occurrenceId = null, CancellationToken cancellationToken = default)
		{
			var data = new JObject();
			data.AddPropertyIfValue("email", email);
			data.AddPropertyIfValue("first_name", firstName);
			data.AddPropertyIfValue("last_name", lastName);

			return _client
				.PostAsync($"webinars/{webinarId}/registrants")
				.WithArgument("occurence_id", occurrenceId)
				.WithJsonBody(data)
				.WithCancellationToken(cancellationToken)
				.AsObject<Registrant>();
		}

		/// <summary>
		/// Approve a registration for a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="registrantId">The registrant ID.</param>
		/// <param name="registrantEmail">The registrant's email address.</param>
		/// <param name="occurrenceId">The webinar occurrence id.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task ApproveRegistrantAsync(long webinarId, string registrantId, string registrantEmail, string occurrenceId = null, CancellationToken cancellationToken = default)
		{
			return ApproveRegistrantsAsync(webinarId, new[] { (registrantId, registrantEmail) }, occurrenceId, cancellationToken);
		}

		/// <summary>
		/// Approve multiple registrations for a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="registrantsInfo">ID and email for each registrant to be approved.</param>
		/// <param name="occurrenceId">The webinar occurrence id.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task ApproveRegistrantsAsync(long webinarId, IEnumerable<(string RegistrantId, string RegistrantEmail)> registrantsInfo, string occurrenceId = null, CancellationToken cancellationToken = default)
		{
			return UpdateRegistrantsStatusAsync(webinarId, registrantsInfo, "approve", occurrenceId, cancellationToken);
		}

		/// <summary>
		/// Reject a registration for a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="registrantId">The registrant ID.</param>
		/// <param name="registrantEmail">The registrant's email address.</param>
		/// <param name="occurrenceId">The webinar occurrence id.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task RejectRegistrantAsync(long webinarId, string registrantId, string registrantEmail, string occurrenceId = null, CancellationToken cancellationToken = default)
		{
			return RejectRegistrantsAsync(webinarId, new[] { (registrantId, registrantEmail) }, occurrenceId, cancellationToken);
		}

		/// <summary>
		/// Reject multiple registrations for a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="registrantsInfo">ID and email for each registrant to be rejected.</param>
		/// <param name="occurrenceId">The webinar occurrence id.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task RejectRegistrantsAsync(long webinarId, IEnumerable<(string RegistrantId, string RegistrantEmail)> registrantsInfo, string occurrenceId = null, CancellationToken cancellationToken = default)
		{
			return UpdateRegistrantsStatusAsync(webinarId, registrantsInfo, "deny", occurrenceId, cancellationToken);
		}

		/// <summary>
		/// Cancel a previously approved registration for a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="registrantId">The registrant ID.</param>
		/// <param name="registrantEmail">The registrant's email address.</param>
		/// <param name="occurrenceId">The webinar occurrence id.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task CancelRegistrantAsync(long webinarId, string registrantId, string registrantEmail, string occurrenceId = null, CancellationToken cancellationToken = default)
		{
			return CancelRegistrantsAsync(webinarId, new[] { (registrantId, registrantEmail) }, occurrenceId, cancellationToken);
		}

		/// <summary>
		/// Cancel multiple previously approved registrations for a webinar.
		/// </summary>
		/// <param name="webinarId">The webinar ID.</param>
		/// <param name="registrantsInfo">ID and email for each registrant to be cancelled.</param>
		/// <param name="occurrenceId">The webinar occurrence id.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task CancelRegistrantsAsync(long webinarId, IEnumerable<(string RegistrantId, string RegistrantEmail)> registrantsInfo, string occurrenceId = null, CancellationToken cancellationToken = default)
		{
			return UpdateRegistrantsStatusAsync(webinarId, registrantsInfo, "cancel", occurrenceId, cancellationToken);
		}

		private Task UpdateRegistrantsStatusAsync(long webinarId, IEnumerable<(string RegistrantId, string RegistrantEmail)> registrantsInfo, string status, string occurrenceId = null, CancellationToken cancellationToken = default)
		{
			var data = new JObject();
			data.AddPropertyIfValue("action", status);
			data.AddPropertyIfValue("registrants", registrantsInfo.Select(ri => new { id = ri.RegistrantId, email = ri.RegistrantEmail }).ToArray());

			return _client
				.PostAsync($"webinars/{webinarId}/registrants/status")
				.WithArgument("occurence_id", occurrenceId)
				.WithJsonBody(data)
				.WithCancellationToken(cancellationToken)
				.AsMessage();
		}
	}
}
