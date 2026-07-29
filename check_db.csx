#r "nuget: MongoDB.Driver, 2.22.0"
using MongoDB.Driver;
using MongoDB.Bson;

var client = new MongoClient("mongodb://localhost:27017");
var db = client.GetDatabase("HealthcareCRM");
var patients = db.GetCollection<BsonDocument>("Patients").Find(new BsonDocument()).ToList();
Console.WriteLine($"Total Patients: {patients.Count}");
foreach(var p in patients) {
    Console.WriteLine($"Patient: {p.GetValue("name", "N/A")} | IsOnline: {p.GetValue("isOnline", "Missing")} | Doc: {p.GetValue("assignedDoctorId", "None")} | Created: {p.GetValue("createdDate", "Missing")}");
}
