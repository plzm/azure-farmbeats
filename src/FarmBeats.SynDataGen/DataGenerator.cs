using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using FarmBeats.Api.Models;

namespace FarmBeats.SynDataGen
{
	public class DataGenerator
	{
		private int _seed = 2358;
		private Faker _faker = new Faker();

		private ICollection<DevicePort> _devicePorts = null;
		private ICollection<string> _sensorMeasureTypes = null;
		private ICollection<string> _sensorMeasureUnits = null;

		public ICollection<DevicePort> DevicePorts
		{
			get
			{
				if (_devicePorts == null)
					_devicePorts = GetDevicePorts();

				return _devicePorts;
			}
		}

		public ICollection<string> SensorMeasureTypes
		{
			get
			{
				if (_sensorMeasureTypes == null)
					_sensorMeasureTypes = GetSensorMeasureTypes();

				return _sensorMeasureTypes;
			}
		}

		public ICollection<string> SensorMeasureUnits
		{
			get
			{
				if (_sensorMeasureUnits == null)
					_sensorMeasureUnits = GetSensorMeasureUnits();

				return _sensorMeasureUnits;
			}
		}

		public DataGenerator()
		{
			Randomizer.Seed = new Random(_seed);
		}

		public IList<FarmRequest> GetFarmRequests(int numberOfItems)
		{
			var faker = new Faker<FarmRequest>()
				.RuleFor(o => o.Description, f => f.Lorem.Sentences())
				.RuleFor(o => o.Geometry, f => this.GetPolygon())
				.RuleFor(o => o.Name, f => f.Company.CompanyName())
				.RuleFor(o => o.Properties, f => this.GetProperties(f.Random.Int(1, 10)))
			;

			return faker.Generate(numberOfItems);
		}

		public IList<DeviceModelRequest> GetDeviceModelRequests(int numberOfItems)
		{
			var faker = new Faker<DeviceModelRequest>()
				.RuleFor(o => o.Description, f => f.Lorem.Sentences())
				.RuleFor(o => o.Manufacturer, f => f.Company.CompanyName())
				.RuleFor(o => o.Name, f => f.Commerce.Product())
				.RuleFor(o => o.Ports, f => this.DevicePorts)
				.RuleFor(o => o.ProductCode, f => f.Commerce.ProductAdjective())
				.RuleFor(o => o.Properties, f => this.GetProperties(f.Random.Int(1, 10)))
				.RuleFor(o => o.Type, f => f.PickRandom<DeviceModelRequestType>())
			;

			return faker.Generate(numberOfItems);
		}

		public IList<DeviceRequest> GetDeviceRequests(int numberOfItems, IEnumerable<string> farmIds, IEnumerable<string> deviceModelIds)
		{
			var faker = new Faker<DeviceRequest>()
				.RuleFor(o => o.Description, f => f.Lorem.Sentences())
				.RuleFor(o => o.DeviceModelId, f => f.PickRandom(deviceModelIds))
				.RuleFor(o => o.HardwareId, f => Guid.NewGuid().ToString())
				.RuleFor(o => o.Location, f => this.GetLocation())
				.RuleFor(o => o.Name, f => f.Commerce.Product())
				.RuleFor(o => o.ParentDeviceId, f => string.Empty)
				.RuleFor(o => o.Properties, f => this.GetProperties(f.Random.Int(1, 10)))
				.RuleFor(o => o.ReportingInterval, f => f.Random.Int(1, 86400))
			;

			if (farmIds != null && farmIds.Count() > 0)
				faker = faker.RuleFor(o => o.FarmId, f => f.PickRandom(farmIds));

			return faker.Generate(numberOfItems);
		}

		public IList<SensorModelRequest> GetSensorModelRequests(int numberOfItems)
		{
			var faker = new Faker<SensorModelRequest>()
				.RuleFor(o => o.Description, f => f.Lorem.Sentences())
				.RuleFor(o => o.Manufacturer, f => f.Company.CompanyName())
				.RuleFor(o => o.Name, f => f.Commerce.Product())
				.RuleFor(o => o.ProductCode, f => f.Commerce.ProductAdjective())
				.RuleFor(o => o.Properties, f => this.GetProperties(f.Random.Int(1, 10)))
				.RuleFor(o => o.SensorMeasures, f => this.GetSensorMeasures(f.Random.Int(2, 8)))
				.RuleFor(o => o.Type, f => f.PickRandom<SensorModelRequestType>())
			;

			return faker.Generate(numberOfItems);
		}

		public IList<SensorRequest> GetSensorRequests(int numberOfItems, IEnumerable<string> deviceIds, IEnumerable<string> sensorModelIds)
		{
			var faker = new Faker<SensorRequest>()
				.RuleFor(o => o.Depth, f => f.Random.Double(0, 120))
				.RuleFor(o => o.Description, f => f.Lorem.Sentences())
				.RuleFor(o => o.DeviceId, f => f.PickRandom(deviceIds))
				.RuleFor(o => o.HardwareId, f => Guid.NewGuid().ToString())
				.RuleFor(o => o.Location, f => this.GetLocation())
				.RuleFor(o => o.Name, f => f.Commerce.Product())
				.RuleFor(o => o.Port, f => f.PickRandom(this.DevicePorts))
				.RuleFor(o => o.Properties, f => this.GetProperties(f.Random.Int(1, 10)))
				.RuleFor(o => o.SensorModelId, f => f.PickRandom(sensorModelIds))
			;

			return faker.Generate(numberOfItems);
		}


		public IDictionary<string, object> GetProperties(int numberOfItems, Func<Faker, string> keySetter = null, Func<Faker, object> valueSetter = null)
		{
			if (keySetter == null)
				keySetter = f => Guid.NewGuid().ToString();

			if (valueSetter == null)
				valueSetter = f => f.Commerce.ProductAdjective();

			var faker = new Faker<DictionaryItem>()
				.RuleFor(o => o.Key, keySetter)
				.RuleFor(o => o.Value, valueSetter)
			;

			// Can't use the more obvious KeyValuePair<string, object> as that's a struct and Bogus requires reference types
			List<DictionaryItem> dictionaryItems = faker.Generate(numberOfItems);

			Dictionary<string, object> result = new Dictionary<string, object>(numberOfItems);

			dictionaryItems.ForEach(di => result.Add(di.Key, di.Value));

			return result;
		}

		public ICollection<DevicePort> GetDevicePorts()
		{
			var faker = new Faker<DevicePort>()
				.RuleFor(o => o.Name, f => f.Commerce.ProductAdjective())
				.RuleFor(o => o.Type, f => f.PickRandom<DevicePortType>())
			;

			return faker.Generate(_faker.Random.Int(2, 13));
		}

		public Polygon GetPolygon()
		{
			int numOfPoints = _faker.Random.Int(4, 8);

			double[][] points = new double[numOfPoints + 1][];

			for (int i = 0; i < numOfPoints; i++)
			{
				double[] point = new double[2] { _faker.Random.Double(-90, 90), _faker.Random.Double(-90, 90) };
				points[i] = point;
			}
			points[numOfPoints] = points[0];

			double[][][] coordinates = new double[][][] { };
			coordinates[0] = points;

			Polygon polygon = new Polygon
			{
				Type = PolygonType.Polygon,
				Coordinates = coordinates
			};

			return polygon;
		}

		public Location GetLocation()
		{
			Location l = new Location
			{
				Elevation = _faker.Random.Double(0, 3000),
				Latitude = _faker.Random.Double(-90, 90),
				Longitude = _faker.Random.Double(-180, 180)
			};

			return l;
		}

		public ICollection<SensorMeasure> GetSensorMeasures(int numberOfItems)
		{
			var faker = new Faker<SensorMeasure>()
				.RuleFor(o => o.AggregationType, f => f.PickRandom<SensorMeasureAggregationType>())
				.RuleFor(o => o.DataType, f => f.PickRandom<SensorMeasureDataType>())
				.RuleFor(o => o.Depth, f => f.Random.Double(0, 120))
				.RuleFor(o => o.Description, f => f.Lorem.Sentences())
				.RuleFor(o => o.Name, f => Guid.NewGuid().ToString())
				.RuleFor(o => o.Type, f => f.PickRandom(this.SensorMeasureTypes))
				.RuleFor(o => o.Unit, f => f.PickRandom(this.SensorMeasureUnits))
			;

			return faker.Generate(numberOfItems);
		}

		public ICollection<string> GetSensorMeasureTypes()
		{
			return new string[]
			{
				"AmbientTemperature",
				"CO2",
				"Depth",
				"ElectricalConductivity",
				"LeafWetness",
				"Length",
				"LiquidLevel",
				"Nitrate",
				"O2",
				"PH",
				"Phosphate",
				"PointInTime",
				"Potassium",
				"Pressure",
				"RainGauge",
				"RelativeHumidity",
				"Salinity",
				"SoilMoisture",
				"SoilTemperature",
				"SolarRadiation",
				"State",
				"TimeDuration",
				"UVRadiation",
				"UVIndex",
				"Volume",
				"WindDirection",
				"WindRun",
				"WindSpeed",
				"Evapotranspiration",
				"PAR"
			};
		}

		public ICollection<string> GetSensorMeasureUnits()
		{
			return new string[]
			{
				"NoUnit",
				"Celsius",
				"Fahrenheit",
				"Kelvin",
				"Rankine",
				"Pascal",
				"Mercury",
				"PSI",
				"MilliMeter",
				"CentiMeter",
				"Meter",
				"Inch",
				"Feet",
				"Mile",
				"KiloMeter",
				"MilesPerHour",
				"MilesPerSecond",
				"KMPerHour",
				"KMPerSecond",
				"MetersPerHour",
				"MetersPerSecond",
				"Degree",
				"WattsPerSquareMeter",
				"KiloWattsPerSquareMeter",
				"MilliWattsPerSquareCentiMeter",
				"MilliJoulesPerSquareCentiMeter",
				"VolumetricWaterContent",
				"Percentage",
				"PartsPerMillion",
				"MicroMol",
				"MicroMolesPerLiter",
				"SiemensPerSquareMeterPerMole",
				"MilliSiemensPerCentiMeter",
				"Centibar",
				"DeciSiemensPerMeter",
				"KiloPascal",
				"VolumetricIonContent",
				"Liter",
				"MilliLiter",
				"Seconds",
				"UnixTimestamp",
				"MicroMolPerMeterSquaredPerSecond",
				"InchesPerHour"
			};
		}

		public class DictionaryItem
		{
			public string Key { get; set; }
			public object Value { get; set; }
		}
	}
}
