# Changelog

## [1.0.0] - 2023-03-21
- Basic movement
- Basic platforms

## [1.1.0] - 2023-04-13
- Support for dynamic moving platforms (With up-axis lockable to make interesting dynamic platforms)
- Support for more modes to interact with dynamic rigidbodies

## [1.1.1] - 2023-08-13
- Fixed a bug where the player can step onto a stair higher than the step height

## [1.2.0] - 2023-08-17
### Add
- Add new way of ground probing and snapping, which is more stable and accurate and works well with
high speed movement
### Removed
- Removed the use of rigidbody component
- Removed the feature to interact with dynamic rigidbodies, instead used a new event to give users
more control over the interaction