# SkyDrop

We want to make Skynet's decentralised storage cloud useful to all mobile users, with a new mobile app for decentralised file transfer. With SkyDrop, you will be able to upload one or more files to Skynet, and share your files using QR codes which point to Skylinks, from any QR scanning camera app.

SkyDrop is intended as a cross platform alternative to AirDrop on iOS. We also support the wider vision of an interoperable, extensible, and decentralised web3, and are open to collaboration and contributors. 

We are still working on a MySky integration, which is challenging to incorporate into .NET without porting SkynetClient, or using an additional Webview that's otherwise not required. This shouldn't be too hard to resolve, so we plan to add importing files from MySky and sharing your files in the app. 

The future feature roadmap of SkyDrop is deliberately open ended, including some privacy features such as E2E encryption of files and media metadata stripping, a web app which encodes existing dweb skylinks into shareable QR codes, and ambitiously we hope to publish SDK offerings as nuget packages for any significant engineering undertaken for porting the existing skynet-js libraries to C#.

Android app ![Build status](https://build.appcenter.ms/v0.1/apps/1cd210b4-00be-4c63-a322-2afc2db6b603/branches/main/badge)
  
iOS app ![Build status](https://build.appcenter.ms/v0.1/apps/7d69bbc9-723d-4bb1-b62f-4c2890c8ab45/branches/main/badge)

[Click to view a SkyGallery album with a demonstration of sending a file from Android to iOS using decentralised QR transfer](https://skygallery.hns.siasky.net/#/a/AAAk0Kqps6NnpZ8bTEnrFI_dpg57n0FwmdG0nyibZxqOhA).
