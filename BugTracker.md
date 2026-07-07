# Bug Tracker

| Bug | Status | Resolution | Priority |
|---|---|---|---|
| 'Patient' does not contain a definition for 'Age' in Index.cshtml and HomeController.cs | Resolved | Replaced `patient.Age` with inline calculation `(DateTime.Now - patient.DateOfBirth).Days / 365` | High |
