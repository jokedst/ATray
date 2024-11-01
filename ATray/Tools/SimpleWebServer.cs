﻿using ATray.Activity;

namespace ATray.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;

    public class WebServer
    {
        private delegate object RequestHandler(HttpListenerContext context, string path);
        private readonly Dictionary<string, RequestHandler> _handlers;
        private readonly JsonSerializerSettings _jsonSettings;
        private HttpListener _listener = new HttpListener();
        private readonly string[] _prefixes;

        public WebServer(params string[] prefixes)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("HttpListener is not supported on this OS");

            // URI "prefix" is the base URI we listen on, e.g. "http://localhost:8080/api/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException(@"Web server must have a prefix to bind to", nameof(prefixes));
            this._prefixes = prefixes;

            this._handlers = new Dictionary<string, RequestHandler>(StringComparer.InvariantCultureIgnoreCase)
            {
                ["GET /"] = this.MainPage,
                ["GET /swagger"] = this.Swagger,
                ["GET /workday"] = this.WorkDay,
            };

            this._jsonSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            this._jsonSettings.Converters.Add(new StringEnumConverter());

            foreach (var s in prefixes.Select(s => s.EndsWith("/") ? s : s + "/"))
                this._listener.Prefixes.Add(s);
        }

        private object WorkDay(HttpListenerContext context, string path)
        {
            var parameters = this.QueryParser(context.Request.Url.Query);
            if (!parameters.TryGetValue("date", out var dateString) || !DateTime.TryParse(dateString, out var day))
            {
                context.Response.StatusCode = 400;
                return new { status = "Bad Request: parameter 'date' was missing or malformed" };
            }
            int blur = 35;
            if (parameters.TryGetValue("blur", out string blurValue))
            {
                if (byte.TryParse(blurValue, out byte blurParam))
                    blur = blurParam;
                else
                {
                    context.Response.StatusCode = 400;
                    return new { status = "Bad Request: parameter 'blur' was malformed" };
                }
            }
                
            var acts = ActivityManager.GetSharedMonthActivities((short)day.Year, (byte)day.Month, "*", blur);
            var dayActs = acts.Values.Where(x => x.Days.ContainsKey((byte) day.Day)).Select(x => x.Days[(byte) day.Day]).ToList();
            var xx = dayActs.SelectMany(x => x.RangesWhere(y => y.WasActive && y.Classification == WorkPlayType.Work));
            var combined = RangeContainer.UintRangeContainer();
            combined.Add(xx);
          var totalTime = SecondToTime( (uint) combined.Sum(x => x.End - x.Start + 1));

            return new
            {
                date = day.ToString("yyyy-MM-dd"),
                totalTime,
                dayNumber =day.Day,
                work =combined.Select(x=>new[]{ SecondToTime(x.Start), SecondToTime(x.End)})
            };
        }
        private string SecondToTime(uint second)
        {
            var minute = second % (60 * 60) / 60;
            return $"{second / (60 * 60):00}:{minute:00}";
        }

        private Dictionary<string, string> QueryParser(string query)
        {
            var result = new Dictionary<string,string>();
            if (string.IsNullOrWhiteSpace(query)) return result;
            if (query.StartsWith("?")) query = query.Substring(1);

            foreach (var parameter in query.Split(new []{'&'}, StringSplitOptions.RemoveEmptyEntries))
            {
                var (key, value) = parameter.Divide('=');
                key=Uri.UnescapeDataString(key);
                result[key] = Uri.UnescapeDataString(value ?? string.Empty);
            }

            return result;
        }

        public void Run()
        {
            try
            {
                this._listener.Start();
            }
            catch (Exception ex)
            {
                Log.ShowError(this, $"API Could not start listening on {string.Join(", ", this._prefixes)}, exception ({ex.GetType().Name}):\n {ex.Message}");
                this._listener = null;
                return;
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                Trace.TraceInformation("Webserver running. HTTP API listening on " + string.Join(", ", this._prefixes));
                try
                {
                    while (this._listener.IsListening)
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext ?? throw new Exception("Could not cast parameter to HttpListenerContext");

                            Log.Info(this, $"API call ({ctx.Request.HttpMethod}) to {ctx.Request.Url.AbsoluteUri}");
                            try
                            {
                                var relativePath = ctx.Request.Url.AbsolutePath;
                             
                                var req = $"{ctx.Request.HttpMethod} {relativePath}";
                                var handler = this._handlers.GetValueOrDefault(req)
                                          //    ?? this._handlers.Where(x=>x.Key.StartsWith(req+"?")).Select(x=>x.Value).FirstOrDefault()
                                              ?? this.Handle404;
                            
                                ctx.Response.ContentType = "application/json";
                                ctx.Response.Headers.Add("Access-Control-Allow-Origin","*");
                                var result = handler(ctx, relativePath);
                                var json = result as string ?? JsonConvert.SerializeObject(result, this._jsonSettings);

                                var buf = Encoding.UTF8.GetBytes(json);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch (Exception ex)
                            {
                                Log.ShowError(this, $"API Exception ({ex.GetType().Name}) on {ctx.Request.HttpMethod} to {ctx.Request.Url.AbsoluteUri}\n{ex.Message}");

                                ctx.Response.StatusCode = 500;
                                ctx.Response.ContentType = "text/plain";
                                var buf = Encoding.UTF8.GetBytes(ex.Message);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, this._listener.GetContext());
                }
                catch (Exception ex)
                {
                    // Just log it
                    Log.ShowError(this, $"Web Server exception ({ex.GetType().Name}):\n{ex.Message}");
                }
            });
        }

        public void Stop()
        {
            this._listener?.Stop();
            this._listener?.Close();
        }
        

        private object MainPage(HttpListenerContext arg1, string arg2)
        {
            return new
            {
                title = "ATray web API",
                methods = this._handlers.Select(x => new { method = x.Key, name = x.Value.Method.Name }),
            };
        }

        private object Swagger(HttpListenerContext context, string urlPath)
        {
            var meh = new JObject
            {
                ["swagger"] = "2.0",
                ["info"] = new JObject
                {
                    ["version"] = "v1",
                    ["title"] = "ATray web API"
                },
                ["host"] = context.Request.UserHostName,
                ["schemes"] = new JArray { "http" },
            };

            var paths = new JObject();
            foreach (var handler in this._handlers)
            {
                var (verb, path) = handler.Key.Divide(' ');
                if (!paths.ContainsKey(path))
                    paths.Add(path, new JObject());
                paths[path][verb.ToLower()] = new JObject {["produces"] = new JArray {"application/json"}};
            }
            meh["paths"] = paths;
            meh["paths"]["/workday"]["get"]["parameters"] = new JArray
            {
                new JObject {["name"] = "date",["description"]="Date to ge work info for", ["in"] = "query",["type"]="string", ["format"] = "date", ["minLength"]=3},
               
                new JObject {["name"] = "blur",["description"]="Amount of blur, 0-100", ["in"] = "query",["type"]="integer", ["minimum"]=0,["maximum"]=100},
            };
            meh["paths"]["/workday"]["get"]["operationId"] = "get_workday";
            meh["paths"]["/workday"]["get"]["tags"] = new JArray("workday");

            return meh;
        }

        private object Handle404(HttpListenerContext context, string path)
        {
            context.Response.StatusCode = 404;
            return new { status = "not found" };
        }
    }
}