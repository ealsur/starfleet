using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;
namespace starfleet.Services{
    /// <summary>
	/// Implementation for System.Web.Caching
	/// </summary>
	/// <remarks>
	/// Create a new instance has no cost.
	/// </remarks>
	public class RedisCache : IRedisCache
	{
        private readonly ConnectionMultiplexer _multiplexer;
		public RedisCache(string accountName, string password){
            var options = new ConfigurationOptions();
			options.EndPoints.Add($"{accountName}.redis.cache.windows.net");
			options.Ssl = true;
			options.Password = password;
			options.ConnectTimeout = 15000;
			options.SyncTimeout = 5000;
			options.AbortOnConnectFail = false;
			_multiplexer = ConnectionMultiplexer.Connect(options);
        }	

		public T Get<T>(string cacheKey)
		{
			if (cacheKey == null)
			{
				return default(T);
			}
			try
			{

				var _cacheDb = _multiplexer.GetDatabase();
				return _cacheDb.Get<T>(cacheKey);

			}
			catch (TimeoutException)
			{
				return default(T);
			}
		}

		public T Put<T>(string cacheKey, T value, TimeSpan relativeExpiration)
		{
			if (cacheKey == null)
			{
				return default(T);
			}
			try
			{
			
				var _cacheDb = _multiplexer.GetDatabase();
				_cacheDb.Set(cacheKey, value, relativeExpiration);
				return value;

			}
			catch (TimeoutException)
			{
				return value;
			}
		}


		public void Evict<T>(string cacheKey)
		{
			if (cacheKey == null)
			{
				return;
			}
			var _cacheDb = _multiplexer.GetDatabase();
			try
			{
				_cacheDb.KeyDelete(cacheKey);
			}
			catch 
			{
				
			}
		}

        public T Push<T>(string id, T value)
        {
            if (id == null)
            {
                return default(T);
            }
            
            var _cacheDb = _multiplexer.GetDatabase();
            _cacheDb.Push(id, value);
            return value;
            
        }

        public T Pop<T>(string id)
        {
            if (id == null)
            {
                return default(T);
            }

            var _cacheDb = _multiplexer.GetDatabase();
            return _cacheDb.Pop<T>(id);

        }

        public List<T> Range<T>(string id, long size)
        {
            if (id == null)
            {
                return default(List<T>);
            }
            try
            {
                var _cacheDb = _multiplexer.GetDatabase();
                return _cacheDb.GetRange<T>(id, size);
            }
            catch (TimeoutException)
            {
                return default(List<T>);
            }
        }

        public long Length(string id)
        {

            if (id == null)
            {
                return 0;
            }
            try
            {


                var _cacheDb = _multiplexer.GetDatabase();
                return _cacheDb.ListLength(id);

            }
            catch (TimeoutException)
            {
                return 0;
            }
        }
	}


    public static class StackExchangeRedisExtensions
	{
		public static T Get<T>(this IDatabase cache, string key)
		{
			return Deserialize<T>(cache.StringGet(key));
		}

		public static object Get(this IDatabase cache, string key)
		{
			return Deserialize<object>(cache.StringGet(key));
		}

		public static void Set(this IDatabase cache, string key, object value)
		{
			cache.StringSet(key, Serialize(value));
		}

		public static void Set(this IDatabase cache, string key, object value, TimeSpan expiration)
		{
			cache.StringSet(key, Serialize(value), expiration);
		}

		private static string Serialize(object o)
		{
			if (o == null)
			{
				return null;
			}

			var serializer = new JsonSerializer
				{
					NullValueHandling = NullValueHandling.Ignore,
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore
				};
			StringWriter stringWriter = new StringWriter();
			JsonWriter jsonWriter = new Newtonsoft.Json.JsonTextWriter(stringWriter);
			serializer.Serialize(jsonWriter, o);
			return stringWriter.ToString();
		}

        public static void Push(this IDatabase cache, string key, object value)
        {
            cache.ListRightPush(key, Serialize(value));
        }

        public static T Pop<T>(this IDatabase cache, string key)
        {
            return Deserialize<T>(cache.ListLeftPop(key));
        }

        public static List<T> GetRange<T>(this IDatabase cache, string key, long size)
        {
            return cache.ListRange(key, 0, size).Select(x=> Deserialize<T>(x)).ToList();
        }


		private static T Deserialize<T>(string objectString)
		{
			if (objectString == null)
			{
				return default(T);
			}
			try
			{
				return JsonConvert.DeserializeObject<T>(objectString,
				            new JsonSerializerSettings()
					            {
						            NullValueHandling = NullValueHandling.Ignore,
						            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
					            });
			}
			catch (Exception ex)
			{
				throw new Exception(objectString, ex);
			}
		}
	}

}