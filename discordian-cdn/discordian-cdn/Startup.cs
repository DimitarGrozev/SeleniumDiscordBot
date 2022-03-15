using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace discordian_cdn
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
			services.AddDirectoryBrowser();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			var fileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, "Discordian"));
			var requestPath = "/Discordian";

			// Enable displaying browser links.
			//app.UseStaticFiles(new StaticFileOptions
			//{
			//	FileProvider = fileProvider,
			//	RequestPath = requestPath
			//});

			//app.UseDirectoryBrowser(new DirectoryBrowserOptions
			//{
			//	FileProvider = fileProvider,
			//	RequestPath = requestPath
			//});
			var updatesFilePath = Path.Combine(env.WebRootPath, "Discordian");

			app.UseFileServer(new FileServerOptions()
			{
				EnableDirectoryBrowsing = true,
				FileProvider = new PhysicalFileProvider(updatesFilePath),
				StaticFileOptions =
				{
					 ServeUnknownFileTypes = true,
					 RequestPath = "/Discordian"
				}
			});

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
			});
		}
	}
}
