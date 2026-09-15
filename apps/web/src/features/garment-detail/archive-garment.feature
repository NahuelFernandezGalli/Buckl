Feature: Archive and restore a garment

  As someone whose closet changes
  I want to take a garment out of my wardrobe without losing its record
  So that the wardrobe shows only what I actually wear

  Scenario: archiving asks for confirmation
    Given a blue top in the wardrobe
    When the user chooses to archive it from its detail
    Then a confirmation asks whether to archive the garment

  Scenario: confirming archives the garment
    Given a blue top in the wardrobe
    And the user chose to archive it from its detail
    When the user confirms
    Then the detail is marked as archived
    And the garment can no longer be edited from the detail
    And the wardrobe no longer lists the blue top

  Scenario: cancelling keeps the garment as it was
    Given a blue top in the wardrobe
    And the user chose to archive it from its detail
    When the user cancels
    Then the detail is not marked as archived
    And the blue top is still in the wardrobe

  Scenario: an archived garment can be restored
    Given an archived blue top
    When the user restores it from its detail
    Then the detail is not marked as archived
    And the blue top is back in the wardrobe

  Scenario: cancelling returns the user to the archive button
    Given a blue top in the wardrobe
    And the user chose to archive it from its detail
    When the user cancels with the keyboard
    Then the archive button has the focus
