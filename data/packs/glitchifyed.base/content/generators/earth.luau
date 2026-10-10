-- type AddData = (id: string, (...any) -> (...any)) -> ()

AddData("glitchifyed.base.generators.earth", function(x: number, y: number, z: number)
    if (y > math.noise(x / 19 + 0.1264273473, z / 19 + 0.112346437327) * 25) then
        return ""
    end
    
    return "glitchifyed.base.voxels.grass"
end)
