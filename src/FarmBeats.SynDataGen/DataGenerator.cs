using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bogus;
using FarmBeats.Api.Models;

namespace FarmBeats.SynDataGen
{
	public class DataGenerator
	{
		private int _seed = 2358;

		public DataGenerator()
		{
			Randomizer.Seed = new Random(_seed);
		}

		public List<DeviceModelRequest> GetDeviceModelRequests(int numberOfItems)
		{
			var faker = new Faker<DeviceModelRequest>()
				.StrictMode(true)
				.RuleFor(o => o.Description, f => f.Lorem.Sentences())
				.RuleFor(o => o.Manufacturer, f => f.Company.CompanyName())
				.RuleFor(o => o.Name, f => f.Commerce.Product())
				.RuleFor(o => o.ProductCode, f => f.Commerce.ProductAdjective())
				.RuleFor(o => o.Type, f => f.PickRandom<DeviceModelRequestType>())
				.RuleFor(o => o.Properties, f => GetProperties(f.Random.Int(1, 10)))
				.RuleFor(o => o.Ports, f => GetDevicePorts(f.Random.Int(1, 5)))
			;

			return faker.Generate(numberOfItems);
		}

		public Dictionary<string, object> GetProperties(int numberOfItems, Func<Faker, string> keySetter = null, Func<Faker, object> valueSetter =  null)
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

		public ICollection<DevicePort> GetDevicePorts(int numberOfItems)
		{
			var faker = new Faker<DevicePort>()
				.RuleFor(o => o.Name, f => f.Commerce.ProductAdjective())
				.RuleFor(o => o.Type, f => f.PickRandom<DevicePortType>())
			;

			return faker.Generate(numberOfItems);
		}
	}

	public class DictionaryItem
	{
		public string Key { get; set; }
		public object Value { get; set; }
	}
}
