﻿using AutoMapper;
using Azure;
using ContactAppMinimalAPI;
using ContactAppMinimalAPI.Entity;
using ContactAppMinimalAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ContactApp with Minimal API",
        Description = "Generated by ASP.NET Core 7.0, Minimal API, Entity Framework Core, SQL Server and Open API."
    });
});

//Add dependency injection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

//Add AutoMapper dependency injection
builder.Services.AddAutoMapper(typeof(MapperConfig));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Database related CRUD operation as usual response
app.MapPost("Contact/AddContact", async (AppDbContext dbContext, [FromBody] ContactDTO _contactDto) =>
{
    Contact contact = new()
    {
        FirstName = _contactDto.FirstName,
        LastName = _contactDto.LastName,
        Office = _contactDto.Office,
        MobileNo = _contactDto.MobileNo,
        Email = _contactDto.Email,
        Birthday = _contactDto.Birthday
    };

    dbContext.Contacts.Add(contact);
    await dbContext.SaveChangesAsync();
    return Results.CreatedAtRoute("SearchContact", new { id = contact.Id }, contact);
}).WithName("AddContact").Accepts<ContactDTO>("application/json").Produces<Contact>(201); ;

app.MapGet("Contact/ContactList", async (AppDbContext dbContext) =>
{
    return await dbContext.Contacts.ToListAsync();
}).WithName("ContactList").Produces<IEnumerable<Contact>>(200);

app.MapGet("Contact/SearchContact/{id}", async (AppDbContext dbContext, int id) =>
{
   return await dbContext.Contacts.FindAsync(id);
}).WithName("SearchContact").Accepts<Int32>("application/json").Produces<Contact>(200);

app.MapPut("Contact/UpdateContact/{id}", async (AppDbContext dbContext, int id, [FromBody] ContactDTO _newContact) =>
{
    try
    {
        Contact oldContact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == id);

        if(oldContact != null)
        {
            oldContact.FirstName=_newContact.FirstName;
            oldContact.LastName = _newContact.LastName;
            oldContact.Office=_newContact.Office;
            oldContact.Email = _newContact.Email;
            oldContact.MobileNo=_newContact.MobileNo;
            oldContact.Birthday=_newContact.Birthday;

            dbContext.Contacts.Update(oldContact);
            await dbContext.SaveChangesAsync();
            return Results.Ok();
        }
        else
        {
            return Results.NotFound();
        }
    }
    catch(Exception ex)
    {
        return Results.BadRequest(ex.Message.ToString());
    }

}).WithName("UpdateContact");

app.MapDelete("Contact/DeleteContact/{id}", async (AppDbContext dbContext, int id) =>
{
    if (await dbContext.Contacts.FindAsync(id) is Contact contact)
    {
        dbContext.Contacts.Remove(contact);
        await dbContext.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();

}).WithName("DeleteContact");

//Database related CRUD operation with custom API response and AutoMapper
app.MapPost("ContactCustomResponse/AddContact", async (AppDbContext dbContext, IMapper _mapper, [FromBody] ContactDTO _contactDto) =>
{
    Contact contact = _mapper.Map<Contact>(_contactDto);
    dbContext.Contacts.Add(contact);
    await dbContext.SaveChangesAsync();

    return Results.CreatedAtRoute("SearchContact", new { id = contact.Id }, contact);
}).WithName("AddContactCustomResponse").Accepts<ContactDTO>("application/json").Produces<Contact>(201); ;

app.MapGet("ContactCustomResponse/ContactList", async (AppDbContext dbContext) =>
{
    APIResponse response = new APIResponse();
    try
    {

        response.IsSuccess = true;
        response.Result = await dbContext.Contacts.ToListAsync();
        response.StatusCode = HttpStatusCode.OK;
    }
    catch (Exception ex)
    {
        response.IsSuccess = false;
        response.StatusCode = HttpStatusCode.BadRequest;
        response.ErrorMessage = ex.Message.ToString();
    }

    return response;
}).WithName("ContactListCustomResponse").Produces<IEnumerable<APIResponse>>(200);

app.MapGet("ContactCustomResponse/SearchContact/{name}", async (AppDbContext dbContext, String name) =>
{
    APIResponse response = new APIResponse();
    try
    {
        IEnumerable<Contact> contacts = await dbContext.Contacts.Where(c => c.FirstName == name).ToListAsync();
        response.IsSuccess = true;
        response.Result = contacts;
        response.StatusCode = HttpStatusCode.OK;
    }
    catch (Exception ex)
    {
        response.IsSuccess=false;
        response.StatusCode = HttpStatusCode.BadRequest;
        response.ErrorMessage=ex.Message.ToString();
    }
    return response;
}).WithName("SearchContactByName").Produces<IEnumerable<APIResponse>>(200);

app.MapPut("ContactCustomResponse/UpdateContact", async (AppDbContext dbContext, [FromBody] Contact _newContact) =>
{
    APIResponse response = new APIResponse();
    try
    {
        Contact oldContact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == _newContact.Id);
        if (oldContact != null)
        {
            dbContext.Contacts.Update(_newContact);
            await dbContext.SaveChangesAsync();

            response.IsSuccess = true;
            response.Result = _newContact;
            response.StatusCode = HttpStatusCode.OK;
        }
        else
        {
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.NotFound;
            response.ErrorMessage = "Contat not found";
        }
    }
    catch (Exception ex)
    {
        response.IsSuccess = false;
        response.StatusCode = HttpStatusCode.BadRequest;
        response.ErrorMessage = ex.Message.ToString();
    }

    return response;
}).WithName("UpdateContactCustomResponse").Accepts<Contact>("application/json").Produces<IEnumerable<APIResponse>>();

app.MapDelete("ContactCustomResponse/DeleteContact/{id}", async (AppDbContext dbContext, int id) =>
{
    APIResponse response = new APIResponse();
    try
    {
        if (await dbContext.Contacts.FindAsync(id) is Contact contact)
        {
            dbContext.Contacts.Remove(contact);
            await dbContext.SaveChangesAsync();

            response.IsSuccess = true;
            response.Result = "Contact has been deleted";
            response.StatusCode = HttpStatusCode.OK;
        }
        else
        {
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.NotFound;
            response.ErrorMessage = "Contact Not found";
        }
    }
    catch (Exception ex)
    {
        response.IsSuccess = false;
        response.StatusCode = HttpStatusCode.BadRequest;
        response.ErrorMessage = ex.Message.ToString();
    }

    return response;
}).WithName("DeleteContactCustomResponse").Produces<IEnumerable<APIResponse>>(200);


app.UseHttpsRedirection();
app.Run();