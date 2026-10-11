local function GetVoxelAtHeight(y: number, height: number)
    if (y > height) then
        return nil
    end
    
    if (y == height) then
        return "grass"
    end
    
    if (y > height - 3) then
        return "dirt"
    end
    
    return "stone"
end

SetMethod("$(pack_id).generators.earth", function(x, y, z)
    local height = math.floor(GetNoise(x * 2, z * 2, 0) * GetNoise(0, x * 0.3, z * 0.3) * GetNoise(x * 0.01, 0, z * 0.01) * 100)
    
    local voxel_id = GetVoxelAtHeight(y, height)
    
    if (not voxel_id) then
        return ""
    end
    
    return `$(pack_id).voxels.{voxel_id}`
end :: GeneratorCallback)
