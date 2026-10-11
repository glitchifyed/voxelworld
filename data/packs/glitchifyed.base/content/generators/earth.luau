-- type AddData = (id: string, (...any) -> (...any)) -> ()

SetMethod("$(pack_id).generators.earth", function(x, y, z)
    local height = 10
    
    if (y > height) then
        return ""
    end
    
    if (y == height) then
        return "$(pack_id).voxels.grass"
    end
    
    if (y > height - 3) then
        return "$(pack_id).voxels.dirt"
    end
    
    return "$(pack_id).voxels.stone"
end :: GeneratorCallback)
