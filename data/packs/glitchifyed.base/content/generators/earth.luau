-- type AddData = (id: string, (...any) -> (...any)) -> ()

SetMethod("$(pack_id).generators.earth", function(x, y, z)
    if (y > math.noise(x / 19 + 0.1264273473, z / 19 + 0.112346437327) * 25) then
        return ""
    end
    
    return "glitchifyed.base.voxels.grass"
end :: GeneratorCallback)

GetGlobal("example")
