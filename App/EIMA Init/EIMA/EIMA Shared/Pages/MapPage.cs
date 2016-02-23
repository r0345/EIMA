using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Plugin.Geolocator;
using XLabs.Forms.Controls;
using TK.CustomMap;
using TK.CustomMap.MapModel;

namespace EIMAMaster
{

	public class MapPage : ContentPage
	{
		TKCustomMap map;
		Position defaultLocation = new Position (39.8, -84.08711552);
		SelectMultipleBasePage<FilterModel> multiPage; //Used in Filter function

		public MapPage ()
		{			
			startAndBindMap();
			setToolBar ();
	
		}

		public void setToolBar(){
			/**
			 * TOOLBAR RELATED CODE
			 */
			//ToolbarItem plusTBI = null;
			ToolbarItem refreshTBI = null;
			ToolbarItem filterTBI = null;
			ToolbarItem mapTypeTBI = null;

			//plusTBI = new ToolbarItem ("", "", () => {addToMap();}, 0, 0);
			refreshTBI = new ToolbarItem ("", "", () => {refreshData();}, 0, 0);
			filterTBI = new ToolbarItem ("", "", () => {filterMapItems();}, 0, 0);
			mapTypeTBI = new ToolbarItem ("", "", () => {changeMap();}, 0, 0);

			//plusTBI.Icon = "Plus.png";
			refreshTBI.Icon = "Refresh.png";
			filterTBI.Icon = "Filter.png";
			mapTypeTBI.Icon = "MapChange.png";

			//Change map type
			ToolbarItems.Add (mapTypeTBI);
			//refresh won't be present for stdAloneUser
			ToolbarItems.Add (refreshTBI);
			ToolbarItems.Add (filterTBI);
			//+ Won't be there for netUser, is there for netMapEdit/netAdmin/stdAloneUser 
			//ToolbarItems.Add (plusTBI);

		}

		public void startAndBindMap(){
			var ms = MapSpan.FromCenterAndRadius (defaultLocation, Distance.FromMiles (0.3));
			map = new TKCustomMap(ms);
			map.IsShowingUser = true;

			var stack = new StackLayout { Spacing = 0 };
			stack.Children.Add(map);
			Content = stack;
			Title = "Maps";
			Icon = "Map.png";

			map.SetBinding(TKCustomMap.CustomPinsProperty, "Pins");
			map.SetBinding(TKCustomMap.MapClickedCommandProperty, "MapClickedCommand");
			map.SetBinding(TKCustomMap.MapLongPressCommandProperty, "MapLongPressCommand");
			map.SetBinding(TKCustomMap.MapCenterProperty, "MapCenter");
			map.SetBinding(TKCustomMap.PinSelectedCommandProperty, "PinSelectedCommand");
			map.SetBinding(TKCustomMap.SelectedPinProperty, "SelectedPin");
			map.SetBinding(TKCustomMap.RoutesProperty, "Routes");
			map.SetBinding(TKCustomMap.PinDragEndCommandProperty, "DragEndCommand");
			map.SetBinding(TKCustomMap.CirclesProperty, "Circles");
			map.SetBinding(TKCustomMap.CalloutClickedCommandProperty, "CalloutClickedCommand");
			map.SetBinding(TKCustomMap.PolylinesProperty, "Lines");
			map.SetBinding(TKCustomMap.PolygonsProperty, "Polygons");
			map.SetBinding(TKCustomMap.MapRegionProperty, "MapRegion");
			map.SetBinding(TKCustomMap.RouteClickedCommandProperty, "RouteClickedCommand");
			map.SetBinding(TKCustomMap.RouteCalculationFinishedCommandProperty, "RouteCalculationFinishedCommand");
			map.SetBinding(TKCustomMap.TilesUrlOptionsProperty, "TilesUrlOptions");
			map.AnimateMapCenterChange = true;
		
			this.BindingContext = new MapModel ();
			getAndSetLocation ();

		}

//		public async void addToMap(){
//			await Navigation.PushAsync (new AddToMapPage());
//		}

		public async void filterMapItems(){
			var items = new List<FilterModel>();
			items.Add (new FilterModel{ Name="Fire"});
			items.Add (new FilterModel{ Name="Police"});
			items.Add (new FilterModel{ Name="Hospital"});
			items.Add (new FilterModel{ Name="Hazmat"});
			items.Add (new FilterModel{ Name="Triage"});
			items.Add (new FilterModel{ Name="Volcano Response"});


			if (multiPage == null)
				multiPage = new SelectMultipleBasePage<FilterModel> (items){ Title = "Filter" };
			multiPage.SelectAll ();//Just for proof of concept. Would need to make this data driven.
			await Navigation.PushAsync (multiPage);

		}

		public void refreshData(){
			//TODO
			//This won't be implemented for a while.
		}

		public void changeMap(){
			if (map.MapType == MapType.Hybrid) {
				map.MapType = MapType.Street;
			} else
				map.MapType = MapType.Hybrid;
		}

		public async void getAndSetLocation(){
			var position = await CrossGeolocator.Current.GetPositionAsync (timeoutMilliseconds: 15000);
			Position parsed = new Position(position.Latitude,position.Longitude);
			var curLoc = MapSpan.FromCenterAndRadius(parsed, Distance.FromMiles(5));
			map.MoveToRegion(curLoc);
		}
	}

}