ent-CrateNPCEmotionalSupport = emotional support pet crate
    .desc = A crate containing a single emotional support pet.
ent-FillNPCEmotionalSupportSafe = { "" }
    .suffix = Safe
    .desc = { "" }
ent-CrateNPCEmotionalSupportSafe = { ent-['FillNPCEmotionalSupportSafe', 'CrateNPCEmotionalSupport'] }

  .desc = { ent-['FillNPCEmotionalSupportSafe', 'CrateNPCEmotionalSupport'].desc }
ent-PetCarrierNPCEmotionalSupportSafe = emotional support pet in a pet carrier

  .desc = { ent-['FillNPCEmotionalSupportSafe', 'PetCarrier'].desc }