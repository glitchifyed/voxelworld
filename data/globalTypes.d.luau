declare Print: (...any) -> ()

declare SetMethod: (id: string, callback: (...any) -> (...any)) -> ()
declare GetGlobal: (id: string) -> string

export type GeneratorCallback = (x: number, y: number, z: number) -> string