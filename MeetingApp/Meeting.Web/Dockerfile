# Stage 1: Build the Angular application
FROM node:18 AS build
WORKDIR /app

# Copy package.json and package-lock.json
COPY Meeting.Web/package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the application
COPY Meeting.Web/. .

# Build the application
RUN npm run build --prod

# Stage 2: Serve the application with Nginx
FROM nginx:alpine
COPY --from=build /app/dist/meeting.web /usr/share/nginx/html

# Copy custom nginx configuration
COPY Meeting.Web/nginx.conf /etc/nginx/nginx.conf

# Expose port 80
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]