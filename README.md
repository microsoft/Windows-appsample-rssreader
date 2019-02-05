---
topic: sample
languages:
- csharp
products:
- windows
- windows-uwp
statusNotificationTargets:
- codefirst
---

<!---
  category: Navigation Data ControlsLayoutAndText NetworkingAndWebServices FilesFoldersAndLibraries
-->

# RssReader sample

A mini-app for retrieving RSS feeds and viewing articles, showing MVVM and design best practices.
Users can specify the URL of a feed, view articles in a WebView control, and save favorite articles to local storage. 
This sample runs on the Universal Windows Platform (UWP). 

![RssReader app displaying some sample feeds](RssReader.png)

## Features

**Note:** Features in this app are subject to change.

RssReader demonstrates:
	
* The navigation menu (hamburger menu) pattern and screen-width adaptivity using the 
  [SplitView](https://msdn.microsoft.com/library/windows/apps/windows.ui.xaml.controls.splitview.aspx) control and the 
  [AdaptiveTrigger](https://msdn.microsoft.com/library/windows/apps/windows.ui.xaml.adaptivetrigger.aspx) class.
* The Syndication APIs ([Windows.Web.Syndication](https://msdn.microsoft.com/library/windows/apps/windows.web.syndication.aspx)) 
  to retrieve RSS feed data. 
* The [DataContractSerializer](https://msdn.microsoft.com/library/windows/apps/system.runtime.serialization.datacontractserializer.aspx) class to save and 
restore app data from local storage.
* C# and XAML using the MVVM design pattern.

### September 2016 update

This update includes:

* General cleanup, commenting, and refactoring for clarity. 
* Improved error handling and performance.
* Fixes for several bugs related to layout, navigation, browser launch, and URL handling. 

### March 2016 update

This update includes:

* A complete redesign of the UI to show effective use of color, type, images, and animated effects.
* Major improvements to layout, navigation, and window-size adaptivity to support small and large screens. 
* Use of the [WebView](https://msdn.microsoft.com/library/windows/apps/windows.ui.xaml.controls.webview.aspx) 
  control to show articles within the app. 
* The ability to rename feeds and to rearrange feeds and favorites. 

We implemented the navigation and layout patterns in this sample using code from the 
[XAML navigation menu](https://github.com/Microsoft/Windows-universal-samples/tree/master/Samples/XamlNavigation) and
[XAML master/detail](https://github.com/Microsoft/Windows-universal-samples/tree/master/Samples/XamlMasterDetail) samples in the 
[Windows-universal-samples](https://github.com/Microsoft/Windows-universal-samples) repo. These samples represent the current
minimum recommendations for these patterns, and the RssReader sample will continue to reflect this guidance in future updates. 

Please report any bugs or suggestions on the [Issues](https://github.com/Microsoft/Windows-appsample-rssreader/issues) list. 
All feedback is welcome!

## Code at a glance

If you're just interested in code snippets for certain API and don't want to browse or run the full sample, 
check out the following files for examples of some highlighted features:

* [FeedView.xaml](RssReader/Views/FeedView.xaml#L25), [AddFeedView.xaml](RssReader/Views/AddFeedView.xaml#L25), 
  [EditFeedsView.xaml](RssReader/Views/EditFeedsView.xaml#L25), and [Styles.xaml](RssReader/Styles/Styles.xaml#L25)
    - Rich UI experiences and XAML resources for colors, templates, and animated effects. 
* [AppShell.xaml](RssReader/AppShell.xaml#L25) and [AppShell.xaml.cs](RssReader/AppShell.xaml.cs#L25)
    - Adapted from the [XAML navigation menu](https://github.com/Microsoft/Windows-universal-samples/tree/master/Samples/XamlNavigation) sample.
    - Use of the [SplitView](https://msdn.microsoft.com/library/windows/apps/windows.ui.xaml.controls.splitview.aspx) control 
      to implement a navigation menu with a hamburger button. 
    - Use of [AdaptiveTrigger](https://msdn.microsoft.com/library/windows/apps/windows.ui.xaml.adaptivetrigger.aspx) with
      [VisualState.Setters](https://msdn.microsoft.com/library/windows/apps/windows.ui.xaml.visualstate.setters.aspx) and 
      [VisualStateManager](https://msdn.microsoft.com/library/windows/apps/windows.ui.xaml.visualstatemanager.aspx) 
      to adjust the navigation menu depending on the current window width. 
    - Code that adjusts header margins depending on the state of the navigation menu and hamburger button. 
    - Keyboard support and Frame navigation.
* [MasterDetailPage.xaml](RssReader/Views/MasterDetailPage.xaml#L25), [MasterDetailPage.xaml.cs](RssReader/Views/MasterDetailPage.xaml.cs#L25),
  [DetailPage.xaml](RssReader/Views/DetailPage.xaml#L25) and [DetailPage.xaml.cs](RssReader/Views/DetailPage.xaml.cs#L25)
    - Adapted from the [XAML master/detail](https://github.com/Microsoft/Windows-universal-samples/tree/master/Samples/XamlMasterDetail) sample.
    - Code that adjusts the display of the articles list, the 
      [WebView](https://msdn.microsoft.com/library/windows/apps/windows.ui.xaml.controls.webview.aspx) showing article content, and 
      the title-bar back button depending on the current window width. 
* [FeedDataSource.cs](RssReader/ViewModels/FeedDataSource.cs#L25) and [Serializer.cs](RssReader/Common/Serializer.cs#L25)
    - Loading default feed data from the app package using [StorageFile.GetFileFromApplicationUriAsync](https://msdn.microsoft.com/library/windows/apps/windows.storage.storagefile.getfilefromapplicationuriasync.aspx).
    - Loading feed and article data from an RSS server using [SyndicationClient.RetrieveFeedAsync](https://msdn.microsoft.com/library/windows/apps/windows.web.syndication.syndicationclient.retrievefeedasync.aspx). 
    - Loading and saving feed and favorites data to/from local storage using [StorageFolder.TryGetItemAsync](https://msdn.microsoft.com/library/windows/apps/windows.storage.storagefolder.trygetitemasync.aspx),
      [StorageFolder.CreateFileAsync](https://msdn.microsoft.com/library/windows/apps/br227250.aspx), 
      and [DataContractSerializer](https://msdn.microsoft.com/library/windows/apps/system.runtime.serialization.datacontractserializer.aspx).

## Universal Windows Platform development

This sample requires [Visual Studio 2017 and the latest version of the Windows 10 SDK](http://go.microsoft.com/fwlink/?LinkID=280676). You can use the free Visual Studio Community Edition to build and run Windows Universal Platform (UWP) apps. 

To get the latest updates to Windows and the development tools, and to help shape their development, join 
the [Windows Insider Program](https://insider.windows.com).

## Running the sample

The default project is RssReader and you can Start Debugging (F5) or Start Without Debugging (Ctrl+F5) to try it out. 
The app will run in the emulator or on physical devices. 

**Note:** This sample assumes you have an internet connection. Also, the platform target currently defaults to ARM, 
so be sure to change that to x64 or x86 if you want to test on a non-ARM device. 

