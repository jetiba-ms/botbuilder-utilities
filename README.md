# botbuilder-utilities

### Bot Authentication with Azure AAD v2
**Important Notes: this is a work in progress repo, probably some code blocks could be refactored and updated**

This sample is to show how to work with the Bot Builder v4 and Topics in order to authenticate the user using the endpoint v2 of Azure Active Directory.

To start developing a new bot using the Bot Builder v4, you can follow the [Getting Started documentation](https://github.com/Microsoft/botbuilder-dotnet/wiki#getting-started).

In order to use the authentication in your bot, you have to:
1. Register the application in Azure AD as it is explained [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-app-registration)
2. Copy information from AAD into Environment Variables as
    * aad:ClientId
    * aad:ClientSecret
    * aad:RedirectUrl
    * aad:Tenant
    * aad:Scopes

The "dirty work" is all done by the class *AuthenticationHelper*. Here you can find 2 methods:
- *GetAuthUrlAsync* to get the url for the authorization
- *GetTokenByAuthCodeAsync* to get the token for the authentication

If you are not used to the Azure Active Directory authorization/authentication flow you can have a look [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-flows)

In the *MessagesController* the conversation is routed on the proper Topic.
There are 2 topics: the *DefaultTopic*, where there could be additional routing logic (for example using [LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/home)), and the *AuthTopic*, where the *SignInCard* is created with the AuthUrl generated in the helper class.
For more details on how to use Topics, refer to [this sample](https://github.com/Microsoft/botbuilder-dotnet/tree/master/samples/AlarmBot).

The *OAuthCallbackController* manages the AAD callback, in order to take the *ConversationReference* from the query and use it to create a sort of *proactive message* to notify the user that the authentication process was successful. Instead, you can not notify the user.
Read also the docs on [Proactive Messages](https://github.com/Microsoft/botbuilder-dotnet/wiki/Proactive-Messaging).

