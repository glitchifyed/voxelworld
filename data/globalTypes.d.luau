declare Print: (...any) -> ()

declare SetMethod: (id: string, callback: (...any) -> (...any)) -> ()
declare GetGlobal: (id: string) -> string
declare GetNoise: (x: number, y: number, z: number) -> number

export type GeneratorCallback = (x: number, y: number, z: number) -> string