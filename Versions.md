# Versions

### 1.0.0 and 1.0.1  

Originals

### 1.1.0

Changes:

1. Added `KeyValue` to `CommandResult` to pass back the database generated key on a *Add*.

2. Added Mapped Handlers and modified `ItemRequestHandler` to handle keyed records. 

### 1.2.0

Minor non breaking fixes.

### 1.3.0

Changes:

1. Removal of any dependencies on the *Blazr.Core* `IEntity` interface.
2. Switch to `FindAsync` to find items in `ItemRequestServerHandler`.
3. Added separate DcoWeatherForecast as domain object for mapped pipeline.
 
### 1.4.2

20-Dec-2023

Changes:

1. Fixes to all the Mapped Server HandlerS to define in and out types in the definitions
2. Added default filter specification to `RecordFilterHandler`.
3. Fixed correct handling of default filters and sorting.  

## 2.0.0

Major rebuild of framework.  Significant breaking changes.

## 2.0.1

Implemented IKeyProvider to manage key functionality generically through registered DI providers.

## 2.0.2

Removal of the `IIdConverter` service.

## 2.0.3/2.0.4

Updates to packages to latest version of Net8 and removal of any packages with advised vulnerabilities.
