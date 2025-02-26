using Hawaii.Test.Models;
using Hawaii.Test.ViewModel;

namespace Hawaii.Test;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		BindingContext = new FeaturesViewModel();
	}
}

