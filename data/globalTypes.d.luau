declare Vector3: {
    zero: Vector3,
    one: Vector3,
    
    xAxis: Vector3,
    yAxis: Vector3,
    zAxis: Vector3,
    
    new: (x: number?, y: number?, z: number?) -> Vector3
}

declare Game: {
    Print: (...any) -> (),
    
    SetMethod: (id: string, callback: (...any) -> (...any)) -> (),
    GetGlobal: (id: string) -> string,
    
    GetNoise: (x: number, y: number, z: number) -> number
}

export type Vector3 = {
    X: number,
    Y: number,
    Z: number,
    
    Magnitude: number,
    Unit: Vector3
}

export type GeneratorCallback = (x: number, y: number, z: number) -> string