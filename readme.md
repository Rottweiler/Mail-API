# Mail API
I'm not held responsible for how you use this code! It's not considered nice to access public mailboxes other than from the Mailinator.com web interface!
### Simple Mailinator.com public API C# implementation
Read emails from any public Mailinator.com inbox, add mail reading support in your .NET applications!
### Features
 * Read mails from a Mailinator.com inbox
 * Get inbox data from a Mailinator.com inbox
 * 1 second timeout inbetween requests to keep public api server happy

### Usage
```c#
using MailAPI;

Mailinator api = new Mailinator("example_inbox"); //example_inbox@mailinator.com

var inbox = api.getInboxStatus();
foreach (var email in inbox.public_msgs)
{
    var easy_mail = api.getEasyMail(email);
    Console.WriteLine("New email from: " + easy_mail.From);
}
```
### Screenshot
[![Example](http://i.imgur.com/Uz0mTnn.png)]