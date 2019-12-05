# Creating a augmented reality app with 4DViews Volumetric Capture

For my second volumetric class I wanted to not only get an engaging capture, but also deploy it to an augmented reality (AR) app on my iPhone. After successfully deploying an AR app using a 4DViews Capture + Unity plugin, I was very aware of the lack of tutorials and documentation around each step of the process. To make easier for my classmates to deploy and showcase their awesome work, I decided to make a step-by-step guide. 

While this is a good, general Vuforia tutorial, this is really meant for people utilizing a [4DViews](https://www.4dviews.com/) system. While I have my full repository uploaded, this guide should be follow step by step. Also, I am using a **Mac** and I'm deploying to **iPhone**. Using windows and android should not only be similar but easier to deploy. 

I am not an iOS developer, so I am encouraging people to submit Pull Request if their code is better. (It probably is)

## Apple Developer Program
**If you are not planning on deploying to iPhone you can ignore this.**

While trying to deploy my X-Code app I was forced to sign up for a developer account. Some people have said I didn't need to spend the 100 dollars to be enrolled in Apple's program, but I haven't figured out how. I believe you need the securiry certificates because we're using "Advanced Features" (AR). 

[Click here to begin enrollment in Apple Development Program](https://developer.apple.com/programs/enroll/)

[Click here to Download X-Code](https://developer.apple.com/download/release/)

## Step 1 
### Capture, Render, and Export your volumetric capture off the 4DViews system.

4DViews exports a filetype .4ds, you need plugins to make Unity understand the system's capture. Typically, 15 second capture, at 30fps would be a little less than half a gigabyte.

## Step 2
### Download Unity Hub + Unity 2018.4.13f1
[Click this link to Create a Unity account (if you don't have one)](https://id.unity.com/en/conversations/9d93c0eb-460b-4339-b9a1-97dc3e29936e012f)

[Click this link to download Unity Hub](https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.dmg?_ga=2.2087473.1036159940.1575574700-22838654.1573235003)

Sign Into Unity Hub with your new account and click License Management. Click "Manual Activation". If this isn't popping up click the settings button on the top right.

![](https://github.com/nicholasoxford/4dviews_vuforia_AR/blob/master/screenshots/Screen%20Shot%202019-12-05%20at%202.50.22%20PM.png?raw=true)

Click Save License Request and save it somewhere easily accessible. Either click on the link below  "Save License Request" or click [here](https://license.unity3d.com/manual).

Upload the file you just saved and click next. 

**On the next page make sure to click "Unity Personal Edition". Then click "I don’t use Unity in a professional capacity."**

Download the _new_ license file in a place you can easily find it and return back to the unity hub. 

On Unity Hub click next and locate the file you just downloaded. It should have an filetype of ".ulf". Click Confirm. Now go to the top right where it says "preferences" and click back. 

Click the "Installs"  tab on the left and then click "Add". 

Locate **Unity 2018.4.13f1 LTS** and click next. Now chose which Modules you want. 
* Check Vuforia Augmented Reality Support
* If you are on Windows and deploying to android chose Windows Build Support and Android Build Support".
* If you are on a Mac and deploying to iPhone chose Mac Build Support and iOS Build Support".

Let it Download.

## Step 3 
### Setting up  Vuforia

[Create a Vuforia Account](https://developer.vuforia.com/vui/auth/register).

[Create A Development Key](https://developer.vuforia.com/vui/develop/licenses)
* Click "Get Development Key" and name it.
* On the top toolbar next to "License Manager", click "Target Manager"

Target Manager is where you are you uploading your "targets", or what you want your AR app to cast on. Now the photos have to be less than 2mbs, so you might have to lower the file size using an app of your choice. 

* Click "Add Database" on the Top Right and name it something relevant. For the "Type" chose "Device".
* Click on the database you just created, and then click "Add Target".
* Chose Single Image as the type and file location
* It might throw an error, usually dealing with size or color space. These are usually an easy fix.
* I would sit the width to "10" and name it. 

Now once you uploaded your photos, refresh the page. It will automatically rate your photo. The rating is based on how many target points it can detect. I would try different images until you get at minimum a three star photo. Now you really only need to upload one photo here, as this tutorial deals with one target. 

* Click "Download Database(ALL)" and save the file somewhere you can easily access it. 

## Step 4
### Downloading the 4DViews Plugin

[Click this link to download the 4DViews Unity plugin](https://www.4dviews.com/file/plugin/Plugin4DS_Unity_v3.0.0.zip)



## Step 5 
### Creating your project + Getting Packages

Go back to Unity Hub and click "Projects" on the left menu bar and then click new. If you have multiple Unity installs make sure to use 2018.4.12f1.

Chose "3D", name it, and store it somewhere other than your Desktop.

Now with Unity open look at the top toolbar for "Window" and chose "Package Manager". 
Search for "Vuforia" and click install.

![](https://github.com/nicholasoxford/4dviews_vuforia_AR/blob/master/screenshots/Screen%20Shot%202019-12-05%20at%203.46.24%20PM.png?raw=true )

Next under Assets click "Import Custom Packages" and import both your database and 4Dviews plugin.
![](https://github.com/nicholasoxford/4dviews_vuforia_AR/blob/master/screenshots/Screen%20Shot%202019-12-05%20at%203.47.41%20PM.png?raw=true)

A window will appear with everything you are importing, just click import.
 

## Step 5 
### Getting Vuforia Working

Make sure you are viewing "Scene" and not "Game"
![](https://github.com/nicholasoxford/4dviews_vuforia_AR/blob/master/screenshots/Screen%20Shot%202019-12-05%20at%203.52.56%20PM.png?raw=true)

First thing you need to do is right click both the main camera and directional light and  delight them. 
Next right click where the camera and light were, find "Vuforia" and click AR Camera. 

![](https://github.com/nicholasoxford/4dviews_vuforia_AR/blob/master/screenshots/Screen%20Shot%202019-12-05%20at%203.50.31%20PM.png?raw=true)

Next click the AR Camera and look over in the "inspector". You should see a button called "Open Vuforia Engine Configuration". Click it. 


* [Go back to your development key and copy the entire string](https://developer.vufooria.com/vui/develop/licenses).
* Find "App License Key" and paste your key. 

Under Databases, what ever the name of the database you imported should appear.
I also uncheck Video Background
