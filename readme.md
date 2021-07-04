/*
 
 ===============================================================================================================
CREATE AN EVENT
===============================================================================================================

POST http://obasen.orientering.se/winsplits/api/events

Adds split times for an event to the WinSplits database.

The HTTP body should contain a result list with split times in IOF XML 3.0 format. The following elements/attributes are required:
/ResultList/Event/Name
/ResultList/Event/Organiser/Name
/ResultList/Event/Organiser/Country[@code]
/ResultList/Event/StartTime/Date
/ResultList/Event/EventClassification

For more information about the IOF XML 3.0 data exchange standard, please refer to https://orienteering.org/resources/it/data-standard-3-0/.

On success, the API returns an IOF XML 3.0 EventList element with a single event. The event ID and password can be used to update or delete the event.
/ResultList/Event/Id: The ID of the newly created event in the WinSplits database. The URL of the event's web page 
is http://obasen.orientering.se/winsplits/online/en/default.asp?page=classes&databaseId={eventId}, where {eventId} is the event's ID.
/ResultList/Extensions/ws:Password: The password of the newly created event in the WinSplits database.



===============================================================================================================
UPDATE AN EVENT
===============================================================================================================

PUT http://obasen.orientering.se/winsplits/api/events/{eventId}/{password}

Updates split times for an event to the WinSplits database.

Parameters:

eventId (required)     The ID of the event to update in the WinSplits database.

password (required)    The password of the event to update in the WinSplits database.

In analogy with event creation, the HTTP body should contain a result list with split times in IOF XML 3.0 format.



===============================================================================================================
DELETE AN EVENT
===============================================================================================================

DELETE http://obasen.orientering.se/winsplits/api/events/{eventId}/{password}

Deletes an event in the WinSplits database.

Parameters:

eventId (required)     The ID of the event to delete in the WinSplits database.

password (required)    The password of the event to delete in the WinSplits database.
 
 
 */