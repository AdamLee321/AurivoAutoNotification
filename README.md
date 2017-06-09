<h1>Aurivo Automatic Notification Service</h1>
<h3>Purpose</h3>
The purpose of this service is to send SMS notifications and group emails to specific alert groups.
<h3>Overview</h3>
Sending an email using a specific email with specific keywords in the subject will relay the communication to all users assigned to the keyword’s alert group. The user can key in a message in the emails body or leave it blank for the alerts default message to be sent.
<h3>EWS API</h3>
The Authentication folder holders the Exchange web services API this allows us to access the chosen mailbox and monitor the emails.
<h3>Vodafone Bulk Message</h3>
This was used to send the SMS messages. This API is very easy to use but can be expensive depending on how many texts you wish to send.
<li>•	Twilio</li>
<li>•	PlaySMS</li>
<li>•	Neon SMS</li>
<h3>SMTP Client</h3>
The simple smtp client was used to send the mail messages and to send a report once the service has been stopped to inform me how many messages were sent and to who.
<h3>Database</h3>
If you are finding it hard to understand thge code its probably because you dont have a database to go with this code. The reason I havent included the databse is for security reasons. If you would like to know how I created the database please get in touch on my website, or leave a comment here on the Hub.

