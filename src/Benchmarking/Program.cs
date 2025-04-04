// See https://aka.ms/new-console-template for more information
using Benchmarking;
using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.Tracing.Parsers;
using System.Globalization;
using System.Numerics;

IEnumerable<Customer> GetCustomers()
{
    string[] lines = File.ReadAllLines("Customers.csv");

    //foreach(var line in lines)
    //{
    //    string[] parts = line.Split(',');
    //    yield return new Customer(parts[0], int.Parse(parts[1], CultureInfo.InvariantCulture));
    //};

    //return lines.Select(line =>
    //{
    //    string[] parts = line.Split(',');
    //    return new Customer(parts[0], int.Parse(parts[1], CultureInfo.InvariantCulture));

    //}).ToList();

    return lines.Select(line =>
    {
        string[] parts = line.Split(',');
        return new Customer(parts[0], int.Parse(parts[1], CultureInfo.InvariantCulture));

    });
}

var customers = GetCustomers().Where(c => c.Age > 20).Take(3);

var numberOfCustomers = customers.Count();
foreach (var customer in customers)
{
    Console.WriteLine($"Name: {customer.Name}, Age: {customer.Age}");
}

record Customer(string Name, int Age);


