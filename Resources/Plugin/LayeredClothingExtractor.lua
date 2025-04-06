if not workspace:HasTag("Rbx2Source_LayeredClothingExtractor") then
    return
end

local Players = game:GetService("Players")
local Selection = game:GetService("Selection")
local userId = workspace:GetAttribute("UserId")

local rig = Players:CreateHumanoidModelFromUserId(userId)
rig.Name = "Rbx2SourceRig"

local dummyTextures = {
    "rbxassetid://78848093925383",
    "rbxassetid://120890729160346",
    "rbxassetid://78868218467223",
    "rbxassetid://127798069773219",
    "rbxassetid://103551854362729",
    "rbxassetid://98219882835394",
    "rbxassetid://129265083766295",
    "rbxassetid://115775189111780",
    "rbxassetid://97148112203914",
    "rbxassetid://95801620423031",
    "rbxassetid://97067783686980",
}

-- Minimize what gets exported beyond the absolute necessities.
for i, desc in rig:GetDescendants() do
    if desc:IsA("BasePart") then
        if desc.Parent == rig or not desc:FindFirstChildOfClass("WrapLayer") then
            desc.Transparency = 1
        end

        if desc:IsA("MeshPart") then
            -- Set texture to a 1x1 pixel.
            -- Mesh needs to have a texture for the UV layout to be preserved.
            if desc.Transparency < 1 then
                local dummyTexture = assert(table.remove(dummyTextures, 1))
                desc.TextureContent = Content.fromUri(dummyTexture)
            end

            local wrapLayer = desc:FindFirstChildOfClass("WrapLayer")
            local att = desc:FindFirstChildOfClass("Attachment")

            if wrapLayer and att then
                task.delay(1, function ()
                    wrapLayer.BindOffset = att.WorldCFrame:Inverse()
                end)
            end
        end

        desc.Locked = true
    elseif desc:IsA("AccessoryDescription") then
		local inst = desc:GetAppliedInstance()
		local handle = inst:FindFirstChild("Handle")

		if handle and handle:IsA("MeshPart") then
            local assetId = tonumber(handle.MeshId:match("%d+$"))

            if assetId then
                handle.Name = tostring(assetId) .. "A";
            end
		end
    elseif desc:IsA("SurfaceAppearance") then
        desc:Destroy()
    end
end

rig:PivotTo(CFrame.identity)
rig.Parent = workspace
task.wait(1)

local camera = workspace.CurrentCamera
camera.CFrame = CFrame.new(0, 0, -5)
camera.Focus = CFrame.identity

Selection:Set({rig})
PluginManager():ExportSelection("Rbx2SourceRig (SAVE TO DESKTOP).obj")
