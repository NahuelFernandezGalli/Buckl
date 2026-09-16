Feature: Photo capture

  As someone adding a garment from the phone
  I want to take a photo or pick one from the gallery
  So that the garment is recognizable in my wardrobe

  Scenario: taking a photo shows a preview
    Given the photo capture is shown
    When the user takes a photo with the camera
    Then a preview of the photo is shown
    And the user can retake it

  Scenario: choosing a photo from the gallery shows a preview
    Given the photo capture is shown
    When the user chooses a photo from the gallery
    Then a preview of the photo is shown

  Scenario: retaking discards the previous photo
    Given the photo capture is shown with a photo already taken
    When the user retakes the photo
    Then no preview is shown
    And the camera and the gallery are offered again
    And the previous preview was released

  Scenario: a file that is not an image is rejected
    Given the photo capture is shown
    When the user chooses a file that is not an image
    Then a message says only images are accepted
    And no preview is shown
