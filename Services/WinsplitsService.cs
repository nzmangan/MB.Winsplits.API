using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IOF.XML.V3;
using Microsoft.Extensions.Logging;

namespace MB.Winsplits.API {
  public class WinsplitsService : IWinsplitsService {
    private readonly ILogger<WinsplitsService> _Logger;
    private readonly HttpClient _HttpClient;

    public WinsplitsService(ILogger<WinsplitsService> logger, HttpClient httpClient) {
      _Logger = logger;
      _HttpClient = httpClient;
    }

    public async Task<APIResponse> Create(byte[] rawFile, ResultListOverrides resultListOverrides = null) {
      var url = "http://obasen.orientering.se/winsplits/api/events";
      return await Update(url, HttpMethod.Post, rawFile, resultListOverrides);
    }

    public async Task<APIResponse> Update(string eventId, string password, byte[] rawFile, ResultListOverrides resultListOverrides = null) {
      var url = $"http://obasen.orientering.se/winsplits/api/events/{eventId}/{password}";
      return await Update(url, HttpMethod.Put, rawFile, resultListOverrides);
    }

    private async Task<APIResponse> Update(string url, HttpMethod httpMethod, byte[] rawFile, ResultListOverrides resultListOverrides = null) {
      ResultList resultList = null;

      try {
        resultList = XMLSerilizer.Deserialize<ResultList>(rawFile);
      } catch (Exception ex) {
        return new APIResponse { Success = false, Message = "Failed to convert xml file. " + ex.ToString() };
      }

      resultList = Update(resultList, resultListOverrides);
      var results = Validate(resultList);

      if (results.Count > 0) {
        return new APIResponse { Success = false, Message = "Not a valid event file or values." };
      }

      using (var requestMessage = new HttpRequestMessage(httpMethod, url)) {
        requestMessage.Content = new StringContent(XMLSerilizer.SerializeObject(resultList), Encoding.UTF8, "text/xml");
        var response = await _HttpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode) {
          return new APIResponse { Success = false, Message = "Uploading failed " + response.ReasonPhrase };
        }

        var stringContent = await response.Content.ReadAsStringAsync();

        _Logger.LogDebug(stringContent);

        var rl = XMLSerilizer.Deserialize<EventList>(stringContent);
        var id = rl.Event[0].Id?.Value;
        var password = rl.Event[0].Extensions.Any.FirstOrDefault(p => p.Name == "ws:Password")?.InnerText;

        return new APIResponse { Success = true, Id = id, Password = password, Link = $"http://obasen.orientering.se/winsplits/online/en/default.asp?page=classes&databaseId={id}" };
      }
    }

    public async Task<APIResponse> Delete(string eventId, string password) {
      var url = $"http://obasen.orientering.se/winsplits/api/events/{eventId}/{password}";

      using (var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url)) {
        var response = await _HttpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode) {
          return new APIResponse { Success = false, Message = "Delete failed " + response.ReasonPhrase };
        }
      }

      return new APIResponse { Success = true, Id = eventId, Message = "Delete succeded!" };
    }

    private List<ValidationResponse> Validate(ResultList resultList) {
      List<ValidationResponse> validationErrors = new();

      if (String.IsNullOrWhiteSpace(resultList.Event.Name)) {
        validationErrors.Add(new ValidationResponse { Message = "Event needs a name." });
      }

      if (String.IsNullOrWhiteSpace(resultList.Event.Organiser[0].Name)) {
        validationErrors.Add(new ValidationResponse { Message = "Event needs an organiser." });
      }

      if (String.IsNullOrWhiteSpace(resultList.Event.Organiser[0].Country?.Code)) {
        validationErrors.Add(new ValidationResponse { Message = "Event needs an country code." });
      }

      if (String.IsNullOrWhiteSpace(resultList.Event.Classification.ToString())) {
        validationErrors.Add(new ValidationResponse { Message = "Event needs an event classification." });
      }

      return validationErrors;
    }

    private ResultList Update(ResultList resultList, ResultListOverrides resultListOverrides) {
      if (resultList.Event.Organiser is null || resultList.Event.Organiser.Length < 1) {
        resultList.Event.Organiser = new List<Organisation> { new Organisation { Country = new Country() } }.ToArray();
      }

      if (resultList.Event.StartTime is null) {
        resultList.Event.StartTime = new DateAndOptionalTime();
      }

      if (resultListOverrides == null) {
        return resultList;
      }

      if (!String.IsNullOrWhiteSpace(resultListOverrides.Name)) {
        resultList.Event.Name = resultListOverrides.Name;
      }

      if (!String.IsNullOrWhiteSpace(resultListOverrides.Organiser)) {
        resultList.Event.Organiser[0].Name = resultListOverrides.Organiser;
      }

      if (!String.IsNullOrWhiteSpace(resultListOverrides.CountryCode)) {
        resultList.Event.Organiser[0].Country.Code = resultListOverrides.CountryCode;
      }

      if (resultListOverrides.Date.HasValue) {
        resultList.Event.StartTime.Date = resultListOverrides.Date.Value;
      }

      if (!String.IsNullOrWhiteSpace(resultListOverrides.EventClassification)) {
        if (!new List<string> { "International", "National", "Regional", "Local", "Club" }.Any(p => p == resultListOverrides.EventClassification)) {
          throw new Exception("Event Classification needs to be International, National, Regional, Local, Club.");
        }

        resultList.Event.Classification = Enum.Parse<EventClassification>(resultListOverrides.EventClassification);
        resultList.Event.ClassificationSpecified = true;
      }

      return resultList;
    }
  }
}