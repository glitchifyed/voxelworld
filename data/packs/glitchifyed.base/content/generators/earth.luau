type AddData = (id: string, (...any) -> (...any)) -> ()

AddData("glitchifyed.base.generators.earth", function(x: number, y: number, z: number)
    -- return if (math.random(1, 2) == 1) then "glitchifyed.base.voxels.grass" else nil
    
    if (y > 0) then
        return ""
    end
    
    return "glitchifyed.base.voxels.grass"
end)
