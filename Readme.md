# PigeonMessenger

Welcome to PigeonMessenger, an API created by Brian Plantico as an at home engineering project during an interview for a Back End Engineering position. PigeonMessenger is built using Azure's cloud native Functions and also utilizes the cloud database provider Snowflake for persisten data storage. The endpoints which you can interact with are outlined below with examples of successful requests and successful responses, however, if you have any questions about this API, please reach out to me directly.

Deployed Link:
https://pigeonmessengerapp.azurewebsites.net/api/v1/messages

#### Brian Plantico: https://github.com/bplantico

### Schema
PigeonMessenger's database schema currently has one table, Messages, though additional tables to persist Users and a corresponding joins table for UsersMessages would also make sense to implement soon with more time.
![PigeonMessenger's DB Schema](https://user-images.githubusercontent.com/43261385/103309281-ee4dca00-49d1-11eb-8443-332199460d1d.png)

### Setup and Configuration


PigeonMessenger provides three endpoints to interact with:
#### Messages
+ [Create Message request](#create_message)
+ [Messages Between Parties request](#between_parties)
+ [Messages for all senders request](#all_senders)

# Configuration
It's not necessary to install and run PigeonMessenger on your local machine since the application's AzureFunctions are deployed and publicly accessible, however, if you're interested in working on the code base the following instructions will help you get started:

Fork and/or clone this repository to your local machine.

Open PigeonMessenger.sln in Visual Studio (or IDE/code editor of your choice that supports running C#/.NET Core code).
The Target Framework used is .Net Core 3.1 (Microsoft.NETCore.App). 

Please message bplantico@gmail.com for credentials to connect to the Snowflake DB.

Copy the local.settings.EXAMPLE.json file and rename it to local.settings.json. In your new file you will need to include values for the keys listed below:

```
"AzureWebJobsStorage": "UseDevelopmentStorage=true",
"FUNCTIONS_WORKER_RUNTIME": "dotnet",
"SnowflakeAccount": "<Snowflake Acct>",
"SnowflakeUser": "<Snowflake Username>",
"SnowflakePassword": "<Snowflake Password>",
"SnowflakeDb": "<Snowflake Db>",
"SnowflakeSchema": "<Snowflake Schema>",
"DefaultResultsLimit": 100
```

You're ready to go! The simplest way to run the code is to use Visual Studio's 'Start' feature, either with debugging enabled (shortcut is to use 'F5'), or without debugging enabled ('Ctrl + F5')

# <a name="create_message"></a>Create Message request
`https://pigeonmessengerapp.azurewebsites.net/api/v1/messages`

A POST request to the messages endpoint returns the id (guid, as a string with hyphens removed) of the newly created Message.

Example Request
```
POST https://pigeonmessengerapp.azurewebsites.net/api/v1/messages

{
    "sender": "elmer",
    "recipient": "bugs",
    "body": "Be vewy, vewy quiet. I'm hunting wabbits.",
    "isPublic": true
}
```
Example Response
```
Status: 201 CREATED

784205cca9f64317993093e972d10be7
```

# <a name="between_parties"></a>Messages Between Parties request
`https://pigeonmessengerapp.azurewebsites.net/api/v1/messages/{recipient}/{sender}`

A GET request to the messages endpoint takes path parameters of a `{recipient}` and `{sender}` and returns the messages between those two parties, limited to the 100 most recent messages by default. The endpoint also accepts an optional query parameter of `since_days_ago`, an integer i.e. `?since_days_ago=2` that is used to filter how many days back you'd like to see messages between the two parties for, with a max of 30 days. If an integer is supplied that's greater than 30, 30 days of messages will be returned. Results are ordered in descending order by when they were created.

Example Request
```
GET https://pigeonmessengerapp.azurewebsites.net/api/v1/messages/elmer/bugs?since_days_ago=2
```
Example Response
```
Status: 200 OK
[
    {
        "id": "784205cca9f64317993093e972d10be7",
        "sender": "elmer",
        "recipient": "bugs",
        "body": "Be vewy, vewy quiet. I'm hunting wabbits.",
        "isPublic": true,
        "createdAt": "2020-12-29T19:55:42",
        "updatedAt": "2020-12-29T19:55:42"
    }
]
```

# <a name="all_senders"></a>Messages for all senders request
`https://pigeonmessengerapp.azurewebsites.net/api/v1/messages`

A GET request to the messages endpoint returns the messages sent from any/all senders, limited to the 100 most recent by default. The endpoint also accepts an optional query parameter of `since_days_ago`, an integer i.e. `?since_days_ago=2` that is used to filter how many days back you'd like to see messages from all senders for, up to a maximum of 30 days. If an integer is supplied that's greater than 30, 30 days of messages will be returned. Results are ordered in descending order by when they were created.

Example Request
```
GET https://pigeonmessengerapp.azurewebsites.net/api/v1/messages
```
Example response
```
Status: 200 OK
[
    {
        "id": "1daad338d3f94cf18e678ab7dc3516a5",
        "sender": "brian",
        "recipient": "bash",
        "body": "I thought it was funny.",
        "isPublic": true,
        "createdAt": "2020-12-29T07:25:46",
        "updatedAt": "2020-12-29T07:25:46"
    },
    {
        "id": "7a3a3814ec06481c8e712616faf85166",
        "sender": "lola",
        "recipient": "bash",
        "body": "We're not friends anymore.",
        "isPublic": true,
        "createdAt": "2020-12-29T06:45:30",
        "updatedAt": "2020-12-29T06:45:30"
    },
    {
        "id": "8246a8bef4e84020806cb7711aa3d729",
        "sender": "lola",
        "recipient": "bash",
        "body": "...smh",
        "isPublic": true,
        "createdAt": "2020-12-29T06:40:48",
        "updatedAt": "2020-12-29T06:40:48"
    },
    {
        "id": "6930206f0f994824b5d66b25be809d82",
        "sender": "bash",
        "recipient": "lola",
        "body": "Not much. What's up with you?",
        "isPublic": true,
        "createdAt": "2020-12-29T06:39:50",
        "updatedAt": "2020-12-29T06:39:50"
    },
    {
        "id": "774615f7e58f4059bd36951f2cdfa60a",
        "sender": "lola",
        "recipient": "bash",
        "body": "Huh? What's updog?",
        "isPublic": true,
        "createdAt": "2020-12-29T06:39:01",
        "updatedAt": "2020-12-29T06:39:01"
    },
    {
        "id": "673480fe1f4442c3bdd609c978c35df6",
        "sender": "bash",
        "recipient": "lola",
        "body": "Have you seen my updog?",
        "isPublic": true,
        "createdAt": "2020-12-29T06:38:25",
        "updatedAt": "2020-12-29T06:38:25"
    }
]
```
