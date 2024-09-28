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
 
## 2.PreReease

Major rebuild of framework.  Significant breaking changes.