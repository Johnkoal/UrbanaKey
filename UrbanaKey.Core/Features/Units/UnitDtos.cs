using System;

namespace UrbanaKey.Core.Features.Units;

public record CreateUnitRequest(
    string Identifier, 
    decimal Coefficient, 
    string UnitType, 
    Guid? ParentUnitId);

public record UpdateUnitRequest(
    string Identifier, 
    decimal Coefficient, 
    string UnitType, 
    bool HasSanctions);

public record UnitResponse(
    Guid Id, 
    string Identifier, 
    decimal Coefficient, 
    string UnitType, 
    bool HasSanctions,
    Guid? ParentUnitId);
