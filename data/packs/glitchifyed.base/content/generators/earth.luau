-- Keep in mind this is run on multiple Luau VMs on multiple different threads.
-- Any globals stored within this are not shared with other chunks.

local GENERATOR_ID = `{Game.GetGlobal("pack_id")}.generators.earth`


local function GetVoxelAtHeight(y: number, height: number): string?
    if (y > height) then
        return
    end
    
    if (y == height) then
        return "grass"
    end
    
    if (y > height - 3) then
        return "dirt"
    end
    
    return "stone"
end


-- Generator function. Returns table of voxel ids by chunk array index.
Game.SetMethod(GENERATOR_ID, function(chunk_x, chunk_y, chunk_z)
    local voxels: {[number]: string} = {}
    
    for x = 0, Game.Generation.ChunkVoxelSize - 1 do
        local global_x = chunk_x + x
        
        for z = 0, Game.Generation.ChunkVoxelSize - 1 do
            local global_z = chunk_z + z
            
            -- Get the height of XZ before going through Y values
            local height = math.floor(Game.Generation.GetNoise(GENERATOR_ID, global_x * 2, global_z * 2, 0) * Game.Generation.GetNoise(0, global_x * 0.3, global_z * 0.3) * Game.Generation.GetNoise(global_x * 0.01, 0, global_z * 0.01) * 100)
            
            for y = 0, Game.Generation.ChunkVoxelSize - 1 do
                local global_y = chunk_y + y
                
                -- Get voxel id (if it exists)
                local voxel_id = GetVoxelAtHeight(global_y, height)
                if (not voxel_id) then
                    continue
                end
                
                -- Get the array index of the position in the chunk
                local chunk_index = Game.Generation.GetChunkIndex(x, y, z)
                
                -- Set the voxel id
                voxels[chunk_index] = `$(pack_id).voxels.{voxel_id}`
            end
        end
    end
    
    return voxels
end :: GeneratorCallback)
