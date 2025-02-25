using Hawaii.Test.Models;
using Hawaii.Test.ViewModel;

namespace Hawaii.Test;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		BindingContext = new FeaturesViewModel
		{
			Features = 
			[
				new Feature()
				{
					Name = "Feature 1",
				},
				new Feature()
				{
					Name = "Feature 2",
				},
			]
		};
	}
}

