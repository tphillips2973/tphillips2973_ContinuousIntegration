# tphillips2973_ContinuousIntegration
 Continuous Integration Assignment, Completed by tphillips2973
 
 Build & Publish Instructions:

Install the .NET Core Hosting Bundle on the IIS server. The bundle installs the .NET Core Runtime, .NET Core Library, and the ASP.NET Core Module. The module allows ASP.NET Core apps to run behind IIS.

Download the installer using the following link:
https://www.microsoft.com/net/permalink/dotnetcore-current-windows-runtime-bundle-installer

Run the installer on the IIS server.

Restart the server or execute 'net stop was /y' followed by 'net start w3svc' in a command shell.


Create the IIS site
 On the IIS server, create a folder to contain the app's published folders and files. In a following step, the folder's path is provided to IIS as the physical path to the app.

 In IIS Manager, open the server's node in the Connections panel. Right-click the Sites folder. Select Add Website from the contextual menu.

 Provide a Site name and set the Physical path to the app's deployment folder that you created. Provide the Binding configuration and create the website by selecting OK.
 
Publish and deploy the app
 Publish an app means to produce a compiled app that can be hosted by a server. Deploy an app means to move the published app to a hosting system. The publish step is handled by the .NET Core SDK, while the deployment step can be handled by a variety of approaches. This tutorial adopts the folder deployment approach, where:

 The app is published to a folder.
 The folder's contents are moved to the IIS site's folder (the Physical path to the site in IIS Manager).
Visual Studio
 Right-click on the project in Solution Explorer and select Publish.
 In the Pick a publish target dialog, select the Folder publish option.
 Set the Folder or File Share path.
 If you created a folder for the IIS site that's available on the development machine as a network share, provide the path to the share. The current user must have write access to publish to the share.
 If you're unable to deploy directly to the IIS site folder on the IIS server, publish to a folder on removeable media and physically move the published app to the IIS site folder on the server, which is the site's Physical path in IIS Manager. Move the contents of the bin/Release/{TARGET FRAMEWORK}/publish folder to the IIS site folder on the server, which is the site's Physical path in IIS Manager.
 
 Browse the website
The app is accessible in a browser after it receives the first request. Make a request to the app at the endpoint binding that you established in IIS Manager for the site.
