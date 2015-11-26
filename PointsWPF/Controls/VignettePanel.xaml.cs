using System.Windows;

namespace PointsOnline
{
	public partial class VignettePanel 
	{
		public VignettePanel()
		{
			InitializeComponent();
        }

		public static readonly DependencyProperty HasVignetteProperty =
			DependencyProperty.Register( "HasVignette",
			typeof( bool ),
			typeof( VignettePanel ),
			new UIPropertyMetadata( true ) );

		public bool HasVignette
		{
			get { return ( bool )GetValue( HasVignetteProperty ); }
			set { SetValue( HasVignetteProperty, value ); }
		}
	}
}
