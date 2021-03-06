﻿using Plugin.Geolocator;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using Xamarin.Forms.Maps;

namespace EIMA
{
	public static class DataNetworkCalls
	{
		public static void updateNetworkedData(){
			var data = DataManager.getInstance();
			var postData = new JObject ();

			postData ["token"] = data.getSecret();

			var accessRes = RestCall.POST (URLs.ACCESSLEVEL, postData);
			if ((bool)accessRes ["result"]) {
				Console.WriteLine (accessRes);

				data.setRole ((string)accessRes ["accessLevel"]);
			}

			if (data.isNoAccess () || data.isStandAlone ()) {
				return;
			}
			SENDLOCATION (postData);

			var MapData = RestCall.POST (URLs.MAPDATA, postData);
			if ((bool)MapData ["result"]) {
				Console.WriteLine (MapData);
				updateMapData (MapData);
			}

			var AlertsData = RestCall.POST (URLs.ALERTS, postData);
			if ((bool)MapData ["result"]) {
				Console.WriteLine (AlertsData);
				updateAlertsData (AlertsData);
			}
				
			if (data.isAdmin ()) {
				//user list
				var result = RestCall.POST (URLs.USERLIST, postData);
				Console.WriteLine (result);

				if(!(bool)result["result"]){
					return;
				}
				var userList = (JArray)result ["userList"];
				var addTo = new List<EIMAUser>();

				foreach (JObject item in userList.Children()) {
					
					var user = new EIMAUser ();
					var unparsed = (int)item ["privLevel"];
//					user.username = (string)item ["username"];
					user.username = (string)item ["name"];
					user.unit = (string)item ["unit"];
					user.unitType = (string)item ["unitType"];
					user.status = (string)item ["status"];
					user.organization = (string)item ["organization"];

					if (unparsed == 3)
						user.level = "admin";
					else if (unparsed == 2) {
						user.level = "mapEditor";
					} else if (unparsed == 1) {
						user.level = "user";
					} else if (unparsed == 0) {
						user.level = "noAccess";
					}

					addTo.Add (user);
				}
				addTo.ForEach(Console.WriteLine);

				data.setUsers (addTo);
			}

			Console.WriteLine (data.getD());


		}

		public static void updateAssets(List<EIMAPin> assets,List<EIMACircle> circles, List<EIMAPolygon> poly){
			var data = DataManager.getInstance();
			var postData = new JObject ();

			if (!data.isAdmin () && !data.isMapEditor ()) {
				return;
			}
			JArray mapAssets = new JArray();
			JArray mapCircles = new JArray();
			JArray mapPolygons = new JArray();

			foreach (EIMAPin asset in assets) {
				if (asset.IsDraggable) {
					JObject toAdd = new JObject ();

					JObject locObject = new JObject ();
					locObject ["lat"] = asset.Position.Latitude;
					locObject ["long"] = asset.Position.Longitude;

					toAdd ["type"] = asset.unitType;
					toAdd ["uid"] = asset.uid;
					toAdd ["name"] = asset.name;
					toAdd ["unit"] = asset.unit;
					toAdd ["status"] = asset.status;
					toAdd ["organization"] = asset.organization;
					toAdd ["location"] = locObject;
					toAdd ["isUser"] = !asset.IsDraggable;
					mapAssets.Add (toAdd);
				}

			}

			foreach (EIMACircle asset in circles) {
				JObject toAdd = new JObject ();

				JObject locObject = new JObject ();
				locObject ["lat"] = asset.Center.Latitude;
				locObject ["long"] = asset.Center.Longitude;

				toAdd ["location"] = locObject;
				toAdd ["type"] = asset.type;
				toAdd ["note"] = asset.note;
				toAdd ["uid"] = asset.uid;
				toAdd ["radius"] = asset.Radius;

				mapCircles.Add (toAdd);
			}

			foreach (EIMAPolygon asset in poly) {
				JObject toAdd = new JObject ();

				JArray coords = new JArray ();
				foreach (Position pos in asset.Coordinates) {
					JObject locObject = new JObject ();

					locObject ["lat"] = pos.Latitude;
					locObject ["long"] = pos.Longitude;

					coords.Add (locObject);
				}
				toAdd ["coords"] = coords;
				toAdd ["type"] = asset.type;
				toAdd ["note"] = asset.note;
				toAdd ["uid"] = asset.uid;

				mapPolygons.Add (toAdd);
			}

			postData ["token"] = data.getSecret();
			postData ["assets"] = mapAssets;
			postData ["circles"] = mapCircles;
			postData ["polygons"] = mapPolygons;

			Console.WriteLine (postData);
			var MapData = RestCall.POST (URLs.MAPEDIT, postData);
			DataNetworkCalls.updateNetworkedData ();
			Console.WriteLine (MapData);
				

		}

		static void updateAlertsData (JObject alertsData)
		{
			var data = DataManager.getInstance ();
			var theList = new List<EIMAAlert> ();
			var alerts = (JArray)alertsData ["alerts"];

			foreach (JObject item in alerts.Children()) {
				var toAdd = new EIMAAlert ();

				toAdd.sender = (string)item ["sender"];
				toAdd.message = (string)item ["message"];
				toAdd.timestamp = (long)item ["timestamp"];

				theList.Add (toAdd);
			}

			data.setAlerts (theList);

		}

		static void updateMapData (JObject mapData)
		{
			var data = DataManager.getInstance ();
			var assetJArr = (JArray)mapData ["mapAssets"];
			var circJArr = (JArray)mapData ["mapCircles"];
			var polyJArr = (JArray)mapData ["mapPolygons"];

			var polyList = new List<EIMAPolygon> ();
			var circleList = new List<EIMACircle> ();
			var assetList = new List<EIMAPin> ();


			foreach (JObject item in polyJArr.Children()) {
				var poly = new EIMAPolygon();
				poly.note = (string)item ["note"];
				poly.uid = (string)item ["uid"];
				poly.type = (string)item ["type"];

				var cordList = new List<Position> ();

				JArray coords = (JArray)item ["points"];
				foreach (JObject pos in coords.Children()) {
					cordList.Add(new Position((double)pos["Latitude"],(double)pos["Longitude"]));
				}
				List<Position> copied = new List<Position>(cordList);
				poly.Coordinates = copied;
				polyList.Add (poly);
			}

			foreach(JObject item in assetJArr.Children()){
				EIMAPin toAdd = new EIMAPin ();

				toAdd.name = (string)item["name"];
				toAdd.uid = (string)item["uid"];
				toAdd.status = (string)item["status"];
				toAdd.organization = (string)item["organization"];
				toAdd.unit = (string)item ["unit"];

				toAdd.Subtitle = "Status:" + toAdd.status;

				JObject pos = (JObject)item ["position"];
				toAdd.Position = new Position ((double)pos["latitude"],(double)pos["longitude"]);
				toAdd.unitType = (string)item ["type"];

				assetList.Add (toAdd);
			}

			foreach (JObject item in circJArr.Children()) {
				var circle = new EIMACircle();
				circle.note = (string)item ["note"];
				circle.uid = (string)item ["uid"];
				circle.Radius = (double)item ["radius"];
				circle.type = (string)item ["type"];
				circle.Center = new Position ((double)item["center"]["lat"],(double)item["center"]["long"]);

				circleList.Add (circle);
			}

			data.setDangerZoneCircle (circleList);
			data.setAssets (assetList);
			data.setDangerZonePoly (polyList);

		}

		async static void SENDLOCATION(JObject postData){
			var postitionData = (JObject) postData.DeepClone ();
			var position = await CrossGeolocator.Current.GetPositionAsync ();

			postitionData ["latit"] = position.Latitude;
			postitionData ["longit"] = position.Longitude;

			RestCall.POST (URLs.LOCATION, postitionData);

		}

	}
}

