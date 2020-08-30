# MondayManager
A voicify integration and app for an Alexa skill and google action to use monday.com APIs

Built originally for the monday.com hackathon - but now a full product you can use in the market place!

## Install

- Using Google Assistant?

Add the Google Assistant app to your monday account: https://auth.monday.com/oauth2/authorize?client_id=3553801cbf0f1c899d9afdeccf9db631&response_type=install

- Using Alexa?

Add the Alexa app to your monday account: https://auth.monday.com/oauth2/authorize?client_id=f3e2110ddd14a7c1d36eb4d048ab15ab&response_type=install

Or go directly to the Action/Skill:
- Google Action: https://assistant.google.com/services/a/uid/000000d2b982bee0
- Alexa: (Currently in certification review)


## Inspiration

I've been a monday.com user for a few years now and use it quite a bit for roadmap planning, task management, team comms, and so much more! I wanted to bridge the gap of what I've been working on in conversational AI and accessibility over the last few years with the same productivity tools I use to work on that technology. 
This led me to a few key points:
- Build something for everyone
- Build something to use anywhere
- Build something actually useful
- Build something production ready
- Teach people along the way

With all of that together, I set out on building the Monday Manager - a voice and conversational assistant that lets you interact with your Monday boards, items, and more!

## What it does

Monday Manager lets you interact with your monday.com account in an entirely new way - with your voice!
It's currently available as an Alexa Skill and a Google Action, but with more platforms to come. To get started simply:
1. Enable the Skill/Action
    - (Once the skill is in the skill store), say "Alexa, enable Monday Manager" or find it in the skill store from the Alexa mobile app. Note: at the point of this project submission, the Alexa skill is still in review and not publicly available.
    - On google say "Hey Google, talk to Monday Manager", or use [this link to the actions directory]( https://assistant.google.com/services/a/uid/000000d2b982bee0)
2. Link your Amazon/Google account to your monday.com account
    - On Alexa, go to the Alexa app, then to the Monday Manager skill and select "settings" to link your account. Then sign in with your Monday account and give the Monday Manager permission to access your boards:
![Alexa account link](https://challengepost-s3-challengepost.netdna-ssl.com/photos/production/software_photos/001/200/841/datas/gallery.jpg)
    - On Google, just say "Talk to Monday Manager" and the sign-in process will start right away.
    - Once you've link your account, you're good to go and no longer need to sign in.
    - Note: you need at least "editor" permissions within your Monday team to use the app
3. Start talking to the Monday Manager and interacting with your boards

You can ask all sorts of questions including major features like:
- Iterating through your boards

![board details](https://challengepost-s3-challengepost.netdna-ssl.com/photos/production/software_photos/001/200/838/datas/gallery.jpg)

- Iterating through each item
- Getting board details
- Adding new items directly into groups

![adding items](https://challengepost-s3-challengepost.netdna-ssl.com/photos/production/software_photos/001/200/839/datas/gallery.jpg)

Then you can immediately see the result in your monday boards:

![added item](https://voicify-prod-files.s3.amazonaws.com/5696e260-bebc-4b63-9bc3-74c781a0375f/87097486-3f46-4586-a99e-b43fa813fae6/Annotation-2020-08-29-1726215.png)


There are also a number of experimental features shown in the video which only work for certain users such as:
- Pushing task dates in bulk
- Getting aggregated item information that you are assigned to
As the system gets smarter, these features will start to be enabled for all users. You can read more about it in the "Challenges I ran into" section.

## How I built it

First off, most of the app has been [built live on my stream](https://twitch.tv/suave_pirate) as a means to help teach developers how to implement similar functionality along the way.

In terms of high-level structure, it basically looks like this:

![arch diagram](https://voicify-prod-files.s3.amazonaws.com/5696e260-bebc-4b63-9bc3-74c781a0375f/0ee386cd-9ba0-4604-abd1-835a3260474b/Annotation-2020-08-29-1726214.png)

The underlying flow is basically:
1. User speaks to their device
2. Device sends request to assistant service
3. Assistant service sends request to the underlying Voicify app
4. Voicify sends webhook requests to the Monday Manager API (when applicable)
5. Monday Manager API talks directly to the Monday GraphQL API
6. Monday Manager API handles business logic for how to turn data into responses
7. Voicify responds to assistant service with the output after mapping it to the proper output
8. Device speaks and displays the result

In terms of the roles:

*The assistant platforms (the actual skill/action manifest) handle*:
- Initial NLU
- Store listings
- Managing endpoint to Voicify

*Voicify handles*:
- Secondary NLU
- Conversation state
- Conversation flow
- Integrations
- Response structures
- Configurations
- Cross-platform deployments

Basically we create conversation items to handle each turn and setup variables that the wehbook can manage filling such as:
![voicify sample](https://voicify-prod-files.s3.amazonaws.com/5696e260-bebc-4b63-9bc3-74c781a0375f/93130760-70ee-47ab-899c-97250d241942/Annotation-2020-08-29-1726216.png)

*The Monday Manager API handles*:
- Mapping input and conversation state from the request
- Communication to the Monday API
- Mapping Monday data to the response structure Voicify expects

The Monday Manager API was built using C# 8 and asp.net core hosted in a linux app service in Azure. Within the project, there's a basic onion design pattern implementation to separate HTTP logic, business logic, and data access logic.
This enables really slim and easy to update and build business logic. For example, the core of letting Voicify get access to the user's current board in context looks like this:

```csharp
public async Task<GeneralFulfillmentResponse> GetCurrentBoard(GeneralWebhookFulfillmentRequest request)
{
    try
    {
        if (string.IsNullOrEmpty(request.OriginalRequest.AccessToken))
            return Unauthorized();

        var currentBoard = await GetCurrentBoardFromRequest(request);
        if (currentBoard == null)
            return Error();

        return new GeneralFulfillmentResponse
        {
            Data = new ContentFulfillmentWebhookData
            {
                Content = BuildBoardResponse(request.Response.Content, currentBoard),
                AdditionalSessionAttributes = new Dictionary<string, object>
                {
                    { SessionAttributes.CurrentBoardSessionAttribute, currentBoard }
                }
            }
        };
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        return Error();
    }
}
```

and the `GetCurrentBoardFromRequest` method basically checks to see if we already have it in session context, or it goes and gets it from the monday API:
```csharp
private async Task<Board[]> GetBoardsFromRequest(GeneralWebhookFulfillmentRequest request)
{
    (request.OriginalRequest.SessionAttributes ?? new Dictionary<string, object>()).TryGetValue(SessionAttributes.BoardsSessionAttribute, out var boardsObj);
    if (boardsObj != null)
        return JsonConvert.DeserializeObject<Board[]>(JsonConvert.SerializeObject(boardsObj));

    var boardsResult = await _mondayDataProvider.GetAllBoards(request.OriginalRequest.AccessToken);
    return boardsResult?.Data;
}

private async Task<Board> GetCurrentBoardFromRequest(GeneralWebhookFulfillmentRequest request)
{
    (request.OriginalRequest.SessionAttributes ?? new Dictionary<string, object>()).TryGetValue(SessionAttributes.CurrentBoardSessionAttribute, out var boardObj);
    if (boardObj != null)
        return JsonConvert.DeserializeObject<Board>(JsonConvert.SerializeObject(boardObj));

    var boards = await GetBoardsFromRequest(request);
    return boards?.FirstOrDefault();
}
```

This layering of abstractions and separation of concerns lets us create brand new features without having to even write code at times, but still allows us to easily implement entirely new sets of logic and functionality if required.

For example, if we wanted to add the ability for the user to say "who am I", and get a response back about their Monday username, it's as simple as:
- Creating a conversation item like this:

![user convo item](https://voicify-prod-files.s3.amazonaws.com/5696e260-bebc-4b63-9bc3-74c781a0375f/840e35ac-e532-4c38-a6a8-f37ec698a4a4/Annotation-2020-08-29-1726217.png)

Attach webhook to it:

![generic webhook](https://voicify-prod-files.s3.amazonaws.com/5696e260-bebc-4b63-9bc3-74c781a0375f/1b6b353c-59ee-4120-ab96-aa28a00c71a3/Annotation-2020-08-29-1726218.png)

***And then it just works!***

One last note is that the entire thing works by having the user link their account to Amazon and Google. This is done by a standard practice called "Account Linking" which just requires some OAuth 2.0 auth code grant flow configuration. Then the user's access token is sent with each request to Voicify. Voicify then sends it to the webhook, which then uses it in the Monday requests.
Here's the general account linking flow according to Alexa:

![account linking](https://m.media-amazon.com/images/G/01/DeveloperBlogs/AlexaBlogs/default/account_linking_1_img.jpg._CB480125555_.jpg)



## Challenges I ran into

The biggest technical hurdle (which I'm working through now) is handling the fact that columns are **entirely** customizable. So, adding features that use those values in bulk require some serious assumptions.
For example, something like "What items of mine are due tomorrow?". Well, we can most certainly handle understanding the goal of that statement, but determining which column(s) actually dictate the "mine" and "tomorrow" part is tricky. Currently, the features that use those work exclusively with some of my board structures to safely handle the assumptions, but my goal is to essentially guess at which column(s) are best for those decisions, and if we aren't sure, just ask the user and remember it for next time.

The other challenge I ran into was filming my fun sample scenarios while my dog was following me around and panting 😂 Hopefully the editing helped there a bit.

## Accomplishments that I'm proud of

There are a few key things that I am super proud of:
1. Successfully building something that functions end to end
2. Getting the Action approved by Google on the first try!
3. Being able to actually teach people along the way while we were building it

All-in-all, it was unbelievably satisfying to actually use the thing for my real day job. Especially adding items to groups with a single command - it immediately showed it's usefulness. 

## What's next for Monday Manager

Tons of stuff! This was not just a hackathon for me, but instead the building of a real product. That's why I submitted it to Alexa and Google for public certification and continue to use it myself everyday!
The biggest next items are:
- More contextual features for users
- Access more parts of the users data like activity, updates, dashboards, etc.
- Learning and managing user preferences and automating that process based off how they interact
- Adding more channels like Bixby, chat bots in MS Teams, Slack, etc.
- Gathering real end-user feedback and iterating on the functionality as we go

## Conclusion

I hope you like the Monday Manager Voice Assistant - it meant a lot to me to be able to find something to build that is meaningful, useful, educational, and accessible to more people while also helping with my own productivity. I'm excited to see the future of the product and continue to build it out myself.
