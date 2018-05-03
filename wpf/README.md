# WPF Integration Sample

The goal of this project to show a simple integration that can be done with WPF. Below you will find some documentation for the code in the repo and how to setup your own project.

# Table of Contents

* [References](#References)
	* [Windows references](#Windowsreferences)
	* [Nuget Packages](#NugetPackages)
* [App Settings](#AppSettings)
* [Basic MVVM](#BasicMVVM)
	* [Navigation](#Navigation)
		* [View \ ViewModel bindings](#ViewViewModelbindings)
		* [IViewModel interface](#IViewModelinterface)
		* [AppNavigationEventArgs](#AppNavigationEventArgs)
		* [Navigation Event Handler](#NavigationEventHandler)
	* [Commands](#Commands)
	* [Property Change](#PropertyChange)
* [OAuth Login \ Logout](#OAuthLoginLogout)
	* [OAuth Service](#OAuthService)
	* [Login Web View](#LoginWebView)
		* [Xaml](#Xaml)
		* [Code Behind](#CodeBehind)
	* [Logout Web View](#LogoutWebView)
		* [Xaml](#Xaml-1)
		* [Code Behind](#CodeBehind-1)
* [API Integration](#APIIntegration)
	* [Configuration of the library](#Configurationofthelibrary)
		* [Method 1 (Static Configuration)](#Method1StaticConfiguration)
		* [Method 2 (Specific Configuration)](#Method2SpecificConfiguration)
	* [Stores API](#StoresAPI)
		* [Retrieve paginated stores (With search query)](#RetrievepaginatedstoresWithsearchquery)
	* [Orders API](#OrdersAPI)
		* [Retrieve paginated orders (With search query)](#RetrievepaginatedordersWithsearchquery)
		* [Accept Order](#AcceptOrder)
		* [Reject Order](#RejectOrder)

## <a name='References'></a>References

Full list of references for succesful compiling of this project, most of them are required for the 3 libraries that we will use. 
* Flipdish - used to call our API
* IdentityModel -- used for OAuth
* System.Reactive -- used for polling of new orders in an event driven way

### <a name='Windowsreferences'></a>Windows references
* *Microsoft.mshtml* required for parsing the request for OAuth with IHtmlDocument3.aspx which provides additional properties and methods of document objects. 
* *System.Configuration* required for reading the App.config file
### <a name='NugetPackages'></a>Nuget Packages
```
Install-Package Flipdish
Install-Package IdentityModel -Version 3.6.1
Install-Package System.Reactive -Version 3.1.1
```
## <a name='AppSettings'></a>App Settings

App settings are located in the App.config file, there are 2 settings there that are required to run the application.

```
  <appSettings>
    <!-- Endpoint is the url used to get your access token -->
    <add key="Endpoint" value="https://api.flipdish.co/"/>
    <!-- ClientId is the app id located in your Flipdish Developer Portal Dashboard -->
    <add key="ClientId" value="[You need to set your client id here]"/>
  </appSettings>
```

In our sample we use a singleton to represent the application settings, the idea here is to be able to call it from any place of our app without adding an extra reference for System.Configuration in every file. 

## <a name='BasicMVVM'></a>Basic MVVM

For the purposes of the sample we use a very basic MVVM implementation. For the purposes of a bigger project we suggest that you use a library that helps you implement MVVM better, for example PRISM for WPF or any other library.

### <a name='Navigation'></a>Navigation

In order to be able to navigate throughout the application we implement a very simple navigation framework that will suffice for the purposes of the sample. 

#### <a name='ViewViewModelbindings'></a>View \ ViewModel bindings

To be able to bind our view models to views we use a simple dictionary that we populate in the MainWindow constructor. It helps to maintain the setup of navigation routes and View \ ViewModel bindings. In our sample, view models control where they want to navigate. 

```
private readonly Dictionary<Type, Grid> _viewViewModelPairs = new Dictionary<Type, Grid>();

public MainWindow()
{
    InitializeComponent();

    //Register basic pairs for navigation
    _viewViewModelPairs.Add(typeof(LoginViewModel), new LoginView());
    _viewViewModelPairs.Add(typeof(OrdersViewModel), new OrdersView());
    _viewViewModelPairs.Add(typeof(OrderReadyToProccessViewModel), new OrderReadyToProccessView());
    _viewViewModelPairs.Add(typeof(StoresViewModel), new StoresView());
    _viewViewModelPairs.Add(typeof(OrderViewModel), new OrderView());

    //Navigate to the first view model, from here on navigation will be handled in the view models
    NavigateTo(this, new AppNavigationEventArgs(new LoginViewModel()));
}
```

#### <a name='IViewModelinterface'></a>IViewModel interface
Our view models that are suitable for navigation will have to implement the IViewModel interface in order to be used as navigation points. The interface is pretty simple, it contains an event that we subscribe to and two functions that allow us to properly initialize the view model at runtime as well as deconstruct it.

```
public interface IViewModel
{
    event EventHandler<AppNavigationEventArgs> RequestNavigation;

    Task NavigateFrom();
    Task NavigateTo();
}
```
#### <a name='AppNavigationEventArgs'></a>AppNavigationEventArgs
This is the arguments that need to be supplied for a navigation request, it only contains an IViewModel that would be used as a DataContext for the view. 

```
public class AppNavigationEventArgs : EventArgs
{
    /// <summary>
    /// This is used for the navigation throughout the application
    /// </summary>
    /// <param name="navigateTo">Registered View Model (can be registered in MainWindow.xaml.cs) for Navigation</param>
    public AppNavigationEventArgs(IViewModel navigateTo)
    {
        ViewModel = navigateTo;
    }

    public IViewModel ViewModel { get; }
}
```

#### <a name='NavigationEventHandler'></a>Navigation Event Handler
This is the handler for navigation request sent by the view model. This will effectively remove the current view from the visual tree and call NavigateFrom on the previous view model. It will also resolve the view associated with the new view model, pair the view to view model and call the NavigateTo method on the new view model. 
```
private async void NavigateTo(object sender, AppNavigationEventArgs navigationArgs)
{
    try
    {
        //Get the type of the view model
        var navigationType = navigationArgs.ViewModel.GetType();

        //Search for the type in the registered pairs
        if (!_viewViewModelPairs.ContainsKey(navigationType))
        {
            throw new ArgumentOutOfRangeException(nameof(navigationArgs.ViewModel));
        }

        //If the triggering, make sure you call navigate from & unsubscribe from the event subscribed to below
        if (sender is IViewModel currentViewModel)
        {
            await currentViewModel.NavigateFrom();
            currentViewModel.RequestNavigation -= NavigateTo;
        }

        //Remove any & all children from our NavigationGrid that we use for navigation
        NavigationGrid.Children.Clear();

        //Get an existing view from the registered view - view model pairs
        var view = _viewViewModelPairs[navigationType];

        //Set the data context of the view to the view model
        view.DataContext = navigationArgs.ViewModel;

        //Subscribe to the navigated event, this will be called when the view model wants to navigate to the next page
        navigationArgs.ViewModel.RequestNavigation += NavigateTo;

        //Add the view that we recieved as a child of the NavigationGrid
        NavigationGrid.Children.Add(view);

        //Call the navigate to, in order to initialize the view model
        await navigationArgs.ViewModel.NavigateTo();
    }
    catch(Exception e)
    {
        Console.WriteLine($"Unhandled exception occured: {e.Message}");
    }
}
```
### <a name='Commands'></a>Commands
In order to make our life easier with Commands we implement a simple RelayCommand class that accepts an action and a predicate. Action is used when the user clicks on the button to which the ICommand is bound. Predicate is used before the user click which allows you to enable \ disable the button, if the predicate returns true then the button is clickable otherwise it should be disabled. 
```
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }
}
```
### <a name='PropertyChange'></a>Property Change
In WPF we need to tell the framework when our properties change so that they could be changed in the UI. For the purposes of this in every ViewModel class we will inherit from BindableBase, we use the [BindableBase](https://github.com/PrismLibrary/Prism/blob/master/Source/Prism/Mvvm/BindableBase.cs) implemented in PRISM framework. In order to not bring the whole PRISM nuget we just use that piece of code in our sample. 

## <a name='OAuthLoginLogout'></a>OAuth Login \ Logout

For succesfully using OAuth we suggest that you use [IdentityModel](https://github.com/IdentityModel/IdentityModel2) the package could be installed from Nuget, you don't need to run this if you installed all packages from the [Nuget Packages Section](#nuget_packages) above.
```
Install-Package IdentityModel -Version 3.6.1
```

### <a name='OAuthService'></a>OAuth Service

OAuthService is a singleton service that will be used as an orchestrator for Login and Logout commands, it creates the relevant queries and executes them in the Web View's explained below. The service itself is very simple, you will need to subscribe and unsubscribe to it's events in your ViewModels NavigateTo & NavigaeFrom.
```
private LoginWebView _login;
private LogoutWebView _logout;
private AuthorizeResponse _authorizeResponse;

private OauthService() { }

public event EventHandler<AuthorizeResponse> LoginDone;
public event EventHandler LogoutDone;

/// <summary>
/// Creates the authorization request and shows the popup with the Web View.
/// When the login is succesfully complete triggers the LoginDone event.
/// </summary>
public void Login(string scope, string responseType)
{
    const string redirectUri = "oob://localhost/wpf.webview.client";

    var request = new RequestUrl($"{AppSettings.Settings.Endpoint}identity/connect/authorize");

    var startUrl = request.CreateAuthorizeUrl(
        clientId: AppSettings.Settings.ClientId,
        responseType: responseType,
        scope: scope,
        redirectUri: redirectUri,
        nonce: CryptoRandom.CreateUniqueId());

    _login = new LoginWebView();
    _login.Done += _login_Done;
    _login.Show();
    _login.Start(new Uri(startUrl), new Uri(redirectUri));

}

private void _login_Done(object sender, AuthorizeResponse e)
{
    _authorizeResponse = e;
    _login.Done -= _login_Done;
    _login.Close();

    LoginDone?.Invoke(sender, e);
}

/// <summary>
/// Creates the end session request and shows the popup with the Web View.
/// When the logout is succesfully complete triggers the LogoutDone event.
/// </summary>
public void Logout()
{
    _logout = new LogoutWebView();
    _logout.Done += _logout_Done;
    _logout.Show();
    _logout.Start(new Uri($"{AppSettings.Settings.Endpoint}identity/connect/endsession"),
        new Uri("https://localhost/identity/logout"));
}

private void _logout_Done(object sender, EventArgs e)
{
    _authorizeResponse = null;
    _logout.Done -= _logout_Done;
    _logout.Close();

    LogoutDone?.Invoke(sender, e);
}
```
### <a name='LoginWebView'></a>Login Web View
In order to authenticate the user with OAuth we need to display Flipdish Login page. For this we are going to use native WPF WebBrowser. For this you will need to create a Window and embed the WebBrowser into it.
#### <a name='Xaml'></a>Xaml
```
<Window x:Class="WpfIntegration.LoginWebView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d"
        Title="LoginWebView" Height="600" Width="800" WindowStartupLocation="CenterScreen">
    <Grid>
        <WebBrowser x:Name="webView" />
    </Grid>
</Window>
```
#### <a name='CodeBehind'></a>Code Behind
```
public partial class LoginWebView
{
    public event EventHandler<AuthorizeResponse> Done;

    Uri _callbackUri;

    public LoginWebView()
    {
        InitializeComponent();
        webView.Navigating += WebView_Navigating;
    }

    public void Start(Uri startUri, Uri callbackUri)
    {
        _callbackUri = callbackUri;
        webView.Navigate(startUri);
    }

    private void Finish(string resultUrl, CancelEventArgs e)
    {
        Hide(e);
        RaiseDone(new AuthorizeResponse(resultUrl));
    }

    private void Hide(CancelEventArgs e)
    {
        e.Cancel = true;
        Visibility = Visibility.Hidden;
    }

    private void RaiseDone(AuthorizeResponse authorizeResponse)
    {
        Done?.Invoke(this, authorizeResponse);
    }

    private void WebView_Navigating(object sender, NavigatingCancelEventArgs e)
    {
        var navigateToCallbackUri = e.Uri.ToString().StartsWith(_callbackUri.AbsoluteUri);
        if (!navigateToCallbackUri)
            return;

        if (e.Uri.AbsoluteUri.Contains("#"))
        {
            Finish(e.Uri.AbsoluteUri, e);
            return;
        }

        var document = (IHTMLDocument3)((WebBrowser)sender).Document;
        var inputElements = document.getElementsByTagName("INPUT").OfType<IHTMLElement>();
        var resultUrl = "?";

        foreach (var input in inputElements)
        {
            resultUrl += input.getAttribute("name") + "=";
            resultUrl += input.getAttribute("value") + "&";
        }

        resultUrl = resultUrl.TrimEnd('&');
        Finish(resultUrl, e);
    }
}
``` 

### <a name='LogoutWebView'></a>Logout Web View
In order to succesfully end session for the user we need to display Flipdish Logout page. For this we are going to use native WPF WebBrowser. For this you will need to create a Window and embed the WebBrowser into it.
#### <a name='Xaml-1'></a>Xaml
```
<Window x:Class="WpfIntegration.LogoutWebView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:WpfIntegration"
        mc:Ignorable="d"
        Title="LogoutWebView" Height="600" Width="800" WindowStartupLocation="CenterScreen">
    <Grid>
        <WebBrowser x:Name="webView" />
    </Grid>
</Window>
```
#### <a name='CodeBehind-1'></a>Code Behind
```
public partial class LogoutWebView
{
    public event EventHandler Done;
    private bool _navigatedToCallBackAlready;

    Uri _callbackUri;

    public LogoutWebView()
    {
        InitializeComponent();
        webView.Navigated += WebView_Navigated;
    }

    private void WebView_Navigated(object sender, NavigationEventArgs e)
    {
        var navigateToCallbackUri = e.Uri.ToString().StartsWith(_callbackUri.AbsoluteUri);

        if (!navigateToCallbackUri)
            return;

        if (_navigatedToCallBackAlready)
        {
            Hide();
            RaiseDone();
        }

        _navigatedToCallBackAlready = true;
    }

    public void Start(Uri startUri, Uri callbackUri)
    {
        _navigatedToCallBackAlready = false;
        _callbackUri = callbackUri;
        webView.Navigate(startUri);
    }
    
    private void RaiseDone()
    {
        Done?.Invoke(this, EventArgs.Empty);
    }
}
```

## <a name='APIIntegration'></a>API Integration

For the API integration we provide a standalone library ([Flipdish](https://github.com/flipdishbytes/api-client-csharp)), we also provide it as a nuget package, you don't need to run this if you installed all packages from the [Nuget Packages Section](#nuget_packages) above.
```
Install-Package Flipdish
```
### <a name='Configurationofthelibrary'></a>Configuration of the library
To use this library you will need to configure either the Library itself or every API instance.

#### <a name='Method1StaticConfiguration'></a>Method 1 (Static Configuration)
You could use a static method of configuration that will be applied to all API instances that are initiated without an explicit configuration instance.
```
//Configures the base path of the API calls
Configuration.Default.BasePath = AppSettings.Settings.Endpoint;
//Configures the AccessToken
Configuration.Default.AccessToken = "[Your access Token]";
//In order to make the calls to the API we need to have a Bearer token associated with our request. or sets the bearer token required to get a reply from our API.
if (Configuration.Default.DefaultHeader.ContainsKey("Authorization"))
{
    Configuration.Default.DefaultHeader["Authorization"] = $"Bearer [Your access Token]";
}
else
{
    Configuration.Default.DefaultHeader.Add("Authorization", $"Bearer [Your access Token]");
}
//After setting up the above setting all the new API Instances will be created without unless directly specified
var storesApi = new StoresApi();
var ordersApi = new OrdersApi();
```
#### <a name='Method2SpecificConfiguration'></a>Method 2 (Specific Configuration)
We could also specify Configuration directly to the constructor of our API objects. This allows for a more flexible configuration.
```
var configuration = new Configuration();
configuration.BasePath = AppSettings.Settings.Endpoint;
configuration.AccessToken = "[Your access Token]";
configuration.DefaultHeader.Add("Authorization", "Bearer [Your access Token]");
//This API instance will run with the above configuration
var ordersApi = new OrdersApi(configuration);
//This API instance will run with Default Static Configuration
var ordersApiDefault = new OrdersApi();
```
### <a name='StoresAPI'></a>Stores API
For the purposes of the demo we only use a few API calls outlined below. If you would like to see further API documentation, you can find it in our [Stores API Reference](https://developers.getflipdish.com/v1.0/reference#stores).

#### <a name='RetrievepaginatedstoresWithsearchquery'></a>Retrieve paginated stores (With search query)
Keep in mind that the maximum amount of stores retrieved per query is 25. Every time we call this we also recieve the total number of stores so we calculate total pages that we could display based on the amount of Stores we want to display per page. 
```
private async Task<IEnumerable<Store>> GetStoresAsync(int page)
{
    var storesApi = new StoresApi();

    //Here we construct and asynchronously execute the GetStores request
    var storesResponse = await storesApi.GetStoresAsync([SearchQuery], page, [StoresPerPage]).ConfigureAwait(false);

    //In the event that there isn't any records TotalRecordCount could be null, so we make sure it has a value before proceeding
    if (storesResponse.TotalRecordCount.HasValue)
    {
        var totalRecords = storesResponse.TotalRecordCount.Value;
        _totalPages = totalRecords / StoresPerPage + (totalRecords % StoresPerPage > 0 ? 1 : 0);
    }

    //We return the stores associated with this request
    return storesResponse.Data;
}
```

### <a name='OrdersAPI'></a>Orders API
For the purposes of the demo we only use a few API calls outlined below. If you would like to see further API documentation, you can find it in our [Orders API Reference](https://developers.getflipdish.com/v1.0/reference#orders).
#### <a name='RetrievepaginatedordersWithsearchquery'></a>Retrieve paginated orders (With search query)
Every time we call this we also recieve the total number of orders so we calculate total pages that we could display based on the amount of orders we want to display per page. 
```
private async Task<IEnumerable<Order>> GetOrdersAsync(int page)
{
    var ordersApi = new OrdersApi();
    var restaurants = new List<int?> { _physicalStoreId };
    var states = new List<string> { "ReadyToProcess" });

    //Here we construct and asynchronously execute the GetOrders request
    var ordersResponse = await ordersApi.GetOrdersAsync(restaurants, states, page, OrdersPerPage).ConfigureAwait(false);

    //In the event that there isn't any records TotalRecordCount could be null, so we make sure it has a value before proceeding
    if (ordersResponse.TotalRecordCount.HasValue)
    {
        var totalRecords = ordersResponse.TotalRecordCount.Value;
        _totalPages = totalRecords / OrdersPerPage + (totalRecords % OrdersPerPage > 0 ? 1 : 0);
    }

    //We return the orders associated with this request
    return ordersResponse.Data;
}
```
#### <a name='AcceptOrder'></a>Accept Order
In order to Accept an order we need to pass the ID of the order and we should create an Accept object which contains only one property: EstimatedMinutesForDelivery
```
var ordersApi = new OrdersApi();
var acceptObject = new Accept([EstimatedMinutesForDelivery]);
await ordersApi.AcceptOrderAsync([OrderId], acceptObject);            
```
#### <a name='RejectOrder'></a>Reject Order
In order to Reject an order we need to pass the ID of the order and we should create a Reject object which contains only one property: RejectReason
```
var ordersApi = new OrdersApi();
var rejectObject = new Reject([RejectReason]);
await ordersApi.RejectOrderAsync([OrderId], rejectObject);  
```
