cd c:\Projects\GitHubSolutions\MetalReleaseTracker\MetalReleaseTracker\
docker-compose down
docker rmi metalreleasetrackerbackgroundservices
docker rmi metalreleasetrackerapi
docker-compose -f docker-compose.yml up
pause